using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Enums;

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

    public Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default)
    {
        // IDs de habitaciones en mantenimiento activo (sin FechaFin)
        var enMantenimiento = Db.HabitacionHistorial
            .Where(hh => hh.FechaFin == null &&
                         hh.EstadoHabitacion == EstadoHabitacion.Mantenimiento)
            .Select(hh => hh.HabitacionId);

        // IDs de habitaciones con reserva activa solapada en el rango
        var conReserva =
            from dr in Db.DetallesReserva
            join r in Db.Reservas on dr.ReservaId equals r.ReservaId
            where r.EstadoReserva != EstadoReserva.Cancelada
               && r.FechaEntrada < salida
               && r.FechaSalida > entrada
            select dr.HabitacionId;

        return Db.Habitaciones
            .AsNoTracking()
            .Where(h => !enMantenimiento.Contains(h.HabitacionId)
                     && !conReserva.Contains(h.HabitacionId))
            .OrderBy(h => h.Piso)
            .ThenBy(h => h.NumeroHabitacion)
            .ToListAsync(ct);
    }
}