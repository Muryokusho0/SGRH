using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class HabitacionRepositoryEF
    : Repository<Habitacion, int>, IHabitacionRepository
{
    public HabitacionRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Habitacion?> GetByIdWithHistorialAsync(
        int id, CancellationToken ct = default)
        => Db.Habitaciones
            .Include(h => h.Historial)
            .FirstOrDefaultAsync(h => h.HabitacionId == id, ct);

    public Task<bool> ExistsByNumeroAsync(
        int numero, CancellationToken ct = default)
        => Db.Habitaciones
            .AnyAsync(h => h.NumeroHabitacion == numero, ct);

    // Sin filtro de categoría — lo usa ReservaDomainPolicy.
    public Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default)
        => GetDisponiblesAsync(entrada, salida, null, ct);

    // Con filtro de categoría — lo usa ListarHabitacionesDisponiblesUseCase.
    public Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaHabitacionId,
        CancellationToken ct = default)
    {
        // Habitaciones en mantenimiento activo (sin FechaFin).
        var enMantenimiento = Db.HabitacionHistorial
            .Where(hh => hh.FechaFin == null &&
                         hh.EstadoHabitacion == EstadoHabitacion.Mantenimiento)
            .Select(hh => hh.HabitacionId);

        // Habitaciones con reserva activa solapada en el rango.
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
        var query = Db.Habitaciones.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoHabitacion>(estado, out var estadoEnum))
        {
            var ids = Db.HabitacionHistorial
                .Where(hh => hh.FechaFin == null && hh.EstadoHabitacion == estadoEnum)
                .Select(hh => hh.HabitacionId);

            query = query.Where(h => ids.Contains(h.HabitacionId));
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