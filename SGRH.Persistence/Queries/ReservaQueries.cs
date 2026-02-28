using SGRH.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;

namespace SGRH.Persistence.Queries;

public sealed class ReservaQueries
{
    private readonly SGRHDbContext _db;

    public ReservaQueries(SGRHDbContext db) => _db = db;

    // Devuelve HabitacionId disponibles en rango [entrada, salida)
    public async Task<List<int>> GetHabitacionesDisponiblesAsync(DateTime entrada, DateTime salida, CancellationToken ct = default)
    {
        // Habitaciones ocupadas por reservas activas (Pendiente/Confirmada) con solapamiento
        var ocupadas = await (from dr in _db.DetallesReserva
                              join r in _db.Reservas on dr.ReservaId equals r.ReservaId
                              where (r.EstadoReserva == SGRH.Domain.Enums.EstadoReserva.Pendiente
                                  || r.EstadoReserva == SGRH.Domain.Enums.EstadoReserva.Confirmada)
                                 && entrada < r.FechaSalida
                                 && salida > r.FechaEntrada
                              select dr.HabitacionId)
                             .Distinct()
                             .ToListAsync(ct);

        // Disponibles = todas - ocupadas
        return await _db.Habitaciones
            .Where(h => !ocupadas.Contains(h.HabitacionId))
            .Select(h => h.HabitacionId)
            .ToListAsync(ct);
    }
}
