using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class HabitacionHistorialRepositoryEF
    : Repository<HabitacionHistorial, int>, IHabitacionHistorialRepository
{
    public HabitacionHistorialRepositoryEF(SGRHDbContext db) : base(db)
    {
    }

    public Task<HabitacionHistorial?> GetVigenteAsync(
        int habitacionId,
        CancellationToken ct = default)
    {
        return Db.HabitacionHistorial
            .AsNoTracking()
            .Where(h =>
                h.HabitacionId == habitacionId &&
                h.FechaFin == null)
            .OrderByDescending(h => h.FechaInicio)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Obtiene todo el historial de una habitación ordenado por fecha.
    /// </summary>
    public Task<List<HabitacionHistorial>> GetByHabitacionAsync(
        int habitacionId,
        CancellationToken ct = default)
    {
        return Db.HabitacionHistorial
            .AsNoTracking()
            .Where(h => h.HabitacionId == habitacionId)
            .OrderByDescending(h => h.FechaInicio)
            .ToListAsync(ct);
    }
}
