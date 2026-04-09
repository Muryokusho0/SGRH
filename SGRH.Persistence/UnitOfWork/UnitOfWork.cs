using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Persistence.Context;
using SGRH.Persistence.Exceptions;

namespace SGRH.Persistence.UnitOfWork;

/// <summary>
/// Coordina la persistencia de cambios y el ciclo de vida de transacciones
/// sobre el <see cref="SGRHDbContext"/>.
///
/// Responsabilidades añadidas frente a la implementación original:
/// - Registra en log el inicio, commit y rollback de cada transacción.
/// - Captura las excepciones de EF Core en SaveChangesAsync y CommitAsync,
///   las registra con contexto completo y las re-lanza como
///   <see cref="PersistenceException"/> para que el middleware pueda mapearlas
///   al código HTTP adecuado (409 Conflict, 400 Bad Request, 500 etc.).
/// - Garantiza el rollback automático en DisposeAsync si la transacción
///   nunca fue confirmada.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly SGRHDbContext _db;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _tx;

    public UnitOfWork(SGRHDbContext db, ILogger<UnitOfWork> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── SaveChangesAsync ──────────────────────────────────────────────────

    /// <summary>
    /// Persiste los cambios pendientes en el contexto.
    /// Registra en log cualquier fallo y lo traduce a
    /// <see cref="PersistenceException"/> con el tipo de error apropiado.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            var count = await _db.SaveChangesAsync(ct);
            _logger.LogDebug(
                "[UnitOfWork] SaveChangesAsync completado. Entidades afectadas: {Count}.", count);
            return count;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "[UnitOfWork] Conflicto de concurrencia en SaveChangesAsync. " +
                "Entidades involucradas: {Entries}.",
                string.Join(", ", ex.Entries.Select(e => e.Entity.GetType().Name)));

            throw new PersistenceException(
                "El registro fue modificado por otra operación simultánea. " +
                "Por favor recarga la información e intenta de nuevo.",
                PersistenceErrorType.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            var errorType = ClasificarDbUpdateException(ex);
            var mensaje = GenerarMensajeDbUpdateException(ex, errorType);

            _logger.LogError(ex,
                "[UnitOfWork] DbUpdateException en SaveChangesAsync. " +
                "Tipo: {ErrorType}. Entidades: {Entries}.",
                errorType,
                string.Join(", ", ex.Entries.Select(e => e.Entity.GetType().Name)));

            throw new PersistenceException(mensaje, errorType, ex);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "[UnitOfWork] SaveChangesAsync cancelado por el cliente.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[UnitOfWork] Error inesperado en SaveChangesAsync.");
            throw new PersistenceException(
                "Error inesperado al guardar los cambios en la base de datos.",
                PersistenceErrorType.GeneralDatabaseError, ex);
        }
    }

    // ── Gestión de transacciones ──────────────────────────────────────────

    /// <summary>
    /// Inicia una transacción de base de datos si no hay una activa.
    /// No inicia una transacción anidada para evitar inconsistencias.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_tx is not null)
        {
            _logger.LogDebug(
                "[UnitOfWork] BeginTransactionAsync: ya existe una transacción activa. " +
                "Se omite el inicio de una nueva.");
            return;
        }

        _tx = await _db.Database.BeginTransactionAsync(ct);
        _logger.LogInformation(
            "[UnitOfWork] Transacción iniciada. Id: {TxId}.", _tx.TransactionId);
    }

    /// <summary>
    /// Persiste los cambios pendientes y confirma la transacción activa.
    /// Si no hay transacción activa, opera sin transacción explícita.
    /// </summary>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_tx is null)
        {
            _logger.LogDebug(
                "[UnitOfWork] CommitAsync sin transacción activa. " +
                "Se guarda directamente con SaveChangesAsync.");
            await SaveChangesAsync(ct);
            return;
        }

        try
        {
            await _tx.CommitAsync(ct);
            _logger.LogInformation(
                "[UnitOfWork] Transacción confirmada. Id: {TxId}.", _tx.TransactionId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "[UnitOfWork] Error al hacer commit de la transacción. " +
                "Se intentará rollback.");

            await TryRollbackAsync();

            throw new PersistenceException(
                "No se pudo confirmar la transacción en la base de datos.",
                PersistenceErrorType.GeneralDatabaseError, ex);
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    /// <summary>
    /// Revierte la transacción activa descartando todos los cambios pendientes.
    /// Si no hay transacción activa, la operación es un no-op.
    /// </summary>
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx is null)
        {
            _logger.LogDebug(
                "[UnitOfWork] RollbackAsync sin transacción activa. No-op.");
            return;
        }

        try
        {
            await _tx.RollbackAsync(ct);
            _logger.LogWarning(
                "[UnitOfWork] Transacción revertida (rollback). Id: {TxId}.",
                _tx.TransactionId);
        }
        catch (Exception ex)
        {
            // El rollback en sí puede fallar (conexión perdida, timeout).
            // Se loggea como crítico porque puede dejar la BD en estado inconsistente.
            _logger.LogCritical(ex,
                "[UnitOfWork] Error crítico al hacer rollback de la transacción. " +
                "La base de datos puede estar en estado inconsistente.");
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    // ── IAsyncDisposable ──────────────────────────────────────────────────

    /// <summary>
    /// Garantiza que cualquier transacción no confirmada se revierta
    /// al liberar el UnitOfWork (por ejemplo si el scope DI se cierra
    /// antes de que el UseCase llame a Commit).
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_tx is not null)
        {
            _logger.LogWarning(
                "[UnitOfWork] DisposeAsync con transacción sin confirmar (Id: {TxId}). " +
                "Se realizará rollback automático.", _tx.TransactionId);
            await TryRollbackAsync();
        }
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    private async Task TryRollbackAsync()
    {
        if (_tx is null) return;
        try
        {
            await _tx.RollbackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "[UnitOfWork] Fallo en rollback de emergencia (DisposeAsync).");
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    private static PersistenceErrorType ClasificarDbUpdateException(DbUpdateException ex)
    {
        if (ex is DbUpdateConcurrencyException)
            return PersistenceErrorType.ConcurrencyConflict;

        var sqlEx = ex.InnerException as Microsoft.Data.SqlClient.SqlException;
        if (sqlEx is null) return PersistenceErrorType.GeneralDatabaseError;

        return sqlEx.Number switch
        {
            2601 or 2627 => PersistenceErrorType.UniqueConstraintViolation,
            547 => PersistenceErrorType.ForeignKeyViolation,
            _ => PersistenceErrorType.GeneralDatabaseError
        };
    }

    private static string GenerarMensajeDbUpdateException(
        DbUpdateException ex, PersistenceErrorType tipo)
        => tipo switch
        {
            PersistenceErrorType.UniqueConstraintViolation =>
                "Ya existe un registro con esos datos. " +
                "Verifica que no haya duplicados antes de continuar.",
            PersistenceErrorType.ForeignKeyViolation =>
                "La operación viola una restricción de integridad referencial. " +
                "Asegúrate de que todos los datos relacionados existan.",
            PersistenceErrorType.ConcurrencyConflict =>
                "El registro fue modificado por otra operación simultánea. " +
                "Por favor recarga la información e intenta de nuevo.",
            _ =>
                "Error al guardar los cambios en la base de datos. " +
                "Por favor intenta de nuevo o contacta al soporte."
        };
}