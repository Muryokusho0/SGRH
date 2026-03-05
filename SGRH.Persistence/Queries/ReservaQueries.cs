using SGRH.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Enums;

namespace SGRH.Persistence.Queries;

public sealed class ReservaQueries
{
    private readonly SGRHDbContext _db;

    public ReservaQueries(SGRHDbContext db) => _db = db;

    public async Task<List<int>> GetHabitacionesDisponiblesAsync(
        DateTime fechaEntrada,
        DateTime fechaSalida,
        int? categoriaHabitacionId = null,
        CancellationToken ct = default)
    {
        if (fechaSalida <= fechaEntrada)
            return [];

        // Habitaciones base (opcional por categoría)
        var habitaciones = _db.Habitaciones.AsNoTracking().AsQueryable();
        if (categoriaHabitacionId.HasValue)
        {
            habitaciones = habitaciones.Where(h => h.CategoriaHabitacionId == categoriaHabitacionId.Value);
        }

        // Reservas que bloquean (solapamiento) + estado (si usas enum)
        var reservasBloqueantes = _db.Reservas.AsNoTracking()
            .Where(r =>
                r.FechaEntrada < fechaSalida &&
                r.FechaSalida > fechaEntrada);

        // Habitaciones ocupadas en esas reservas
        var ocupadasIds = await (
            from dr in _db.DetallesReserva.AsNoTracking()
            join r in reservasBloqueantes on dr.ReservaId equals r.ReservaId
            select dr.HabitacionId
        ).Distinct().ToListAsync(ct);

        // Disponibles = todas - ocupadas
        return await habitaciones
            .Where(h => !ocupadasIds.Contains(h.HabitacionId))
            .Select(h => h.HabitacionId)
            .ToListAsync(ct);
    }
}