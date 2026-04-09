using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class HabitacionRepositoryEF
    : Repository<Habitacion, int>, IHabitacionRepository
{
    public HabitacionRepositoryEF(SGRHDbContext db, ILogger<HabitacionRepositoryEF> logger) : base(db, logger) { }

    public Task<Habitacion?> GetByIdWithHistorialAsync(
        int id, CancellationToken ct = default)
        => Db.Habitaciones
            .Include(h => h.Historial)
            .FirstOrDefaultAsync(h => h.HabitacionId == id, ct);

    public Task<bool> ExistsByNumeroAsync(
        int numero, CancellationToken ct = default)
        => Db.Habitaciones.AnyAsync(h => h.NumeroHabitacion == numero, ct);

    // Busca por número de habitación (el número visible al cliente).
    // NumeroHabitacion tiene índice único en la BD.
    public Task<Habitacion?> GetByNumeroAsync(
        int numero, CancellationToken ct = default)
        => Db.Habitaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.NumeroHabitacion == numero, ct);

    public Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default)
        => GetDisponiblesAsync(entrada, salida, null, ct);

    public Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaHabitacionId,
        CancellationToken ct = default)
    {
        var enMantenimiento = Db.HabitacionHistorial
            .Where(hh => hh.FechaFin == null &&
                         hh.EstadoHabitacion == EstadoHabitacion.Mantenimiento)
            .Select(hh => hh.HabitacionId);

        var conReserva =
            from dr in Db.DetallesReserva
            join r in Db.Reservas on dr.ReservaId equals r.ReservaId
            where r.EstadoReserva != EstadoReserva.Cancelada
               && r.FechaEntrada < salida
               && r.FechaSalida > entrada
            select dr.HabitacionId;

        var query = Db.Habitaciones
            .AsNoTracking()
            .Where(h => !enMantenimiento.Contains(h.HabitacionId)
                     && !conReserva.Contains(h.HabitacionId));

        if (categoriaHabitacionId.HasValue)
            query = query.Where(h => h.CategoriaHabitacionId == categoriaHabitacionId.Value);

        return query
            .OrderBy(h => h.Piso)
            .ThenBy(h => h.NumeroHabitacion)
            .ToListAsync(ct);
    }

    public Task<List<Habitacion>> BuscarAsync(
        string? estado, int? categoriaId, int? piso,
        CancellationToken ct = default)
    {
        var query = Db.Habitaciones
            .Include(h => h.Historial)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoHabitacion>(estado, ignoreCase: true, out var estadoEnum))
        {
            query = query.Where(h =>
                h.Historial.Any(hh => hh.FechaFin == null &&
                                      hh.EstadoHabitacion == estadoEnum));
        }

        if (categoriaId.HasValue)
            query = query.Where(h => h.CategoriaHabitacionId == categoriaId.Value);

        if (piso.HasValue)
            query = query.Where(h => h.Piso == piso.Value);

        return query
            .OrderBy(h => h.Piso)
            .ThenBy(h => h.NumeroHabitacion)
            .ToListAsync(ct);
    }
}