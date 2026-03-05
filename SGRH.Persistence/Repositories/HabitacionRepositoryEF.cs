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

namespace SGRH.Persistence.Repositories;

public sealed class HabitacionRepositoryEF
    : Repository<Habitacion, int>, IHabitacionRepository
{
    public HabitacionRepositoryEF(SGRHDbContext db) : base(db)
    {
    }

    public override Task<Habitacion?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return Db.Habitaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.HabitacionId == id, ct);
    }

    public Task<List<Habitacion>> GetByCategoriaAsync(
        int categoriaId,
        CancellationToken ct = default)
    {
        return Db.Habitaciones
            .AsNoTracking()
            .Where(h => h.CategoriaHabitacionId == categoriaId)
            .OrderBy(h => h.HabitacionId)
            .ToListAsync(ct);
    }

    public Task<List<Habitacion>> GetAllAsync(
        CancellationToken ct = default)
    {
        return Db.Habitaciones
            .AsNoTracking()
            .OrderBy(h => h.HabitacionId)
            .ToListAsync(ct);
    }

    public Task<Habitacion?> GetWithHistorialAsync(int habitacionId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}