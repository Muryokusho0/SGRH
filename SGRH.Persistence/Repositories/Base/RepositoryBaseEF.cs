using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Persistence.Context;
using SGRH.Persistence.Exceptions;

namespace SGRH.Persistence.Repositories.Base;

/// <summary>
/// Implementación base de IRepository usando Entity Framework Core.
/// Todas las implementaciones concretas heredan de esta clase.
///
/// Responsabilidades:
/// - Encapsula el acceso al DbContext y al DbSet.
/// - Registra en log las operaciones de escritura a nivel Debug.
/// - Captura y traduce las excepciones de EF Core antes de propagarlas,
///   preservando la excepción original como InnerException.
/// </summary>
public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly SGRHDbContext Db;
    protected readonly DbSet<TEntity> Set;

    // ILogger inyectado en el constructor de cada repositorio concreto
    // mediante el parámetro genérico para que el nombre de categoría en los
    // logs refleje el tipo concreto (ej. "ReservaRepositoryEF") y no la
    // clase base abstracta.
    private readonly ILogger _logger;

    protected Repository(SGRHDbContext db, ILogger logger)
    {
        Db = db;
        Set = db.Set<TEntity>();
        _logger = logger;
    }

    /// <inheritdoc />
    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    /// <inheritdoc />
    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => Set.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[Persistence] Add {Entity}.", typeof(TEntity).Name);
        try
        {
            await Set.AddAsync(entity, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "[Persistence] Error al preparar Add de {Entity}.", typeof(TEntity).Name);
            throw WrapException(ex, $"No se pudo agregar la entidad {typeof(TEntity).Name}.");
        }
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        _logger.LogDebug(
            "[Persistence] Update {Entity}.", typeof(TEntity).Name);
        try
        {
            Set.Update(entity);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "[Persistence] Error al preparar Update de {Entity}.", typeof(TEntity).Name);
            throw WrapException(ex, $"No se pudo actualizar la entidad {typeof(TEntity).Name}.");
        }
    }

    /// <inheritdoc />
    public virtual void Delete(TEntity entity)
    {
        _logger.LogDebug(
            "[Persistence] Delete {Entity}.", typeof(TEntity).Name);
        try
        {
            Set.Remove(entity);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "[Persistence] Error al preparar Delete de {Entity}.", typeof(TEntity).Name);
            throw WrapException(ex, $"No se pudo eliminar la entidad {typeof(TEntity).Name}.");
        }
    }

    /// <summary>
    /// Acceso al IQueryable para consultas personalizadas en subclases.
    /// No forma parte del contrato público IRepository.
    /// </summary>
    protected IQueryable<TEntity> Query()
        => Set.AsQueryable();

    // ── Traducción de excepciones EF Core ─────────────────────────────────

    /// <summary>
    /// Traduce las excepciones de EF Core a <see cref="PersistenceException"/>
    /// con un <see cref="PersistenceErrorType"/> apropiado para que el middleware
    /// pueda mapearlas al código HTTP correcto.
    /// </summary>
    protected static PersistenceException WrapException(Exception ex, string context)
    {
        if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            return new PersistenceException(
                $"{context} El registro fue modificado por otra operación concurrente.",
                PersistenceErrorType.ConcurrencyConflict, ex);

        if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Inspeccionar el código de error de SQL Server
            var sqlEx = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException;
            if (sqlEx is not null)
            {
                // 2601: Cannot insert duplicate key row (unique index)
                // 2627: Violation of PRIMARY KEY / UNIQUE constraint
                if (sqlEx.Number is 2601 or 2627)
                    return new PersistenceException(
                        $"{context} Ya existe un registro con esos datos (restricción de unicidad).",
                        PersistenceErrorType.UniqueConstraintViolation, ex);

                // 547: The INSERT/UPDATE/DELETE statement conflicted with FK constraint
                if (sqlEx.Number == 547)
                    return new PersistenceException(
                        $"{context} Hay una restricción de relación que impide la operación.",
                        PersistenceErrorType.ForeignKeyViolation, ex);
            }

            return new PersistenceException(
                $"{context} Error al guardar los cambios en la base de datos.",
                PersistenceErrorType.GeneralDatabaseError, ex);
        }

        // Para cualquier otra excepción no anticipada, re-envolver con contexto
        return new PersistenceException(
            $"{context} Error inesperado en la capa de persistencia.",
            PersistenceErrorType.GeneralDatabaseError, ex);
    }
}