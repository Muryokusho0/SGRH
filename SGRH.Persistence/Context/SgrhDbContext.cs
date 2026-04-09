using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Domain.Entities.Clientes;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Entities.Servicios;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Persistence.Configurations;
using SGRH.Persistence.Exceptions;

namespace SGRH.Persistence.Context;

/// <summary>
/// Contexto principal de Entity Framework Core para SGRH.
/// 
/// Mejoras de logging y manejo de errores:
/// - Inyecta <see cref="ILogger{SGRHDbContext}"/> para registrar eventos
///   relevantes del contexto (recuento de entidades guardadas, errores).
/// - Sobreescribe <see cref="SaveChangesAsync"/> para añadir logging
///   y traducir excepciones EF a <see cref="PersistenceException"/>.
///   Esto complementa el manejo en <see cref="SGRH.Persistence.UnitOfWork.UnitOfWork"/>:
///   el UnitOfWork envuelve la lógica de transacción, mientras el contexto
///   registra el intento de escritura a nivel más fino.
/// </summary>
public class SGRHDbContext : DbContext
{
    private readonly ILogger<SGRHDbContext> _logger;

    public SGRHDbContext(
        DbContextOptions<SGRHDbContext> options,
        ILogger<SGRHDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    // ─── CLIENTES ────────────────────────────────────────────────
    public DbSet<Cliente> Clientes => Set<Cliente>();

    // ─── HABITACIONES ────────────────────────────────────────────
    public DbSet<CategoriaHabitacion> CategoriasHabitacion => Set<CategoriaHabitacion>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<HabitacionHistorial> HabitacionHistorial => Set<HabitacionHistorial>();
    public DbSet<TarifaTemporada> TarifasTemporada => Set<TarifaTemporada>();

    // ─── RESERVAS ────────────────────────────────────────────────
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<DetalleReserva> DetallesReserva => Set<DetalleReserva>();
    public DbSet<ReservaServicioAdicional> ReservaServiciosAdicionales
        => Set<ReservaServicioAdicional>();

    // ─── SERVICIOS ───────────────────────────────────────────────
    public DbSet<ServicioAdicional> ServiciosAdicionales => Set<ServicioAdicional>();
    public DbSet<ServicioCategoriaPrecio> ServicioCategoriaPrecios
        => Set<ServicioCategoriaPrecio>();

    public DbSet<ServicioTemporada> ServicioTemporadas => Set<ServicioTemporada>();

    // ─── TEMPORADAS ──────────────────────────────────────────────
    public DbSet<Temporada> Temporadas => Set<Temporada>();

    // ─── SEGURIDAD ───────────────────────────────────────────────
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // ─── AUDITORÍA ───────────────────────────────────────────────
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();
    public DbSet<AuditoriaEventoDetalle> AuditoriaEventoDetalles
        => Set<AuditoriaEventoDetalle>();

    // ── SaveChangesAsync con logging ──────────────────────────────────────

    /// <summary>
    /// Sobreescritura de SaveChangesAsync que añade:
    /// 1. Log de diagnóstico con el recuento de entidades en cada estado.
    /// 2. Log de warning ante excepciones de concurrencia.
    /// 3. Log de error ante cualquier DbUpdateException.
    /// Las excepciones NO se consumen aquí — se dejan propagar para que
    /// el UnitOfWork las capture y traduzca a PersistenceException.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // ── Diagnóstico de entidades pendientes ───────────────────────────
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var resumen = ChangeTracker.Entries()
                .GroupBy(e => e.State)
                .Select(g => $"{g.Key}={g.Count()}")
                .ToList();

            if (resumen.Count > 0)
                _logger.LogDebug(
                    "[DbContext] SaveChangesAsync iniciado. Entidades pendientes: {Resumen}.",
                    string.Join(", ", resumen));
        }

        try
        {
            var count = await base.SaveChangesAsync(cancellationToken);
            _logger.LogDebug(
                "[DbContext] SaveChangesAsync exitoso. Filas afectadas: {Count}.", count);
            return count;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entidades = ex.Entries
                .Select(e => e.Entity.GetType().Name)
                .ToList();

            _logger.LogWarning(ex,
                "[DbContext] Conflicto de concurrencia al guardar. " +
                "Entidades afectadas: {Entidades}.", string.Join(", ", entidades));
            throw; // El UnitOfWork lo envuelve como PersistenceException
        }
        catch (DbUpdateException ex)
        {
            var entidades = ex.Entries
                .Select(e => e.Entity.GetType().Name)
                .ToList();

            _logger.LogError(ex,
                "[DbContext] DbUpdateException al guardar. " +
                "Entidades afectadas: {Entidades}. " +
                "Inner: {InnerMessage}.",
                string.Join(", ", entidades),
                ex.InnerException?.Message ?? ex.Message);
            throw; // El UnitOfWork lo envuelve como PersistenceException
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SGRHDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}