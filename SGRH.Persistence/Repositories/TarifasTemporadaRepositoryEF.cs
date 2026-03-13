using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class TarifaTemporadaRepositoryEF
    : Repository<TarifaTemporada, int>, ITarifaTemporadaRepository
{
    public TarifaTemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<bool> ExisteParaCategoriaYTemporadaAsync(
        int categoriaHabitacionId, int temporadaId, CancellationToken ct = default)
        => Db.TarifasTemporada
            .AnyAsync(t =>
                t.CategoriaHabitacionId == categoriaHabitacionId &&
                t.TemporadaId == temporadaId, ct);

    public Task<TarifaTemporada?> GetTarifaAsync(
        int categoriaId, int temporadaId, CancellationToken ct = default)
        => Db.TarifasTemporada
            .FirstOrDefaultAsync(t =>
                t.CategoriaHabitacionId == categoriaId &&
                t.TemporadaId == temporadaId, ct);

    public Task<List<TarifaTemporada>> BuscarAsync(
        int? categoriaId, int? temporadaId, CancellationToken ct = default)
    {
        var query = Db.TarifasTemporada.AsNoTracking();

        if (categoriaId.HasValue)
            query = query.Where(t => t.CategoriaHabitacionId == categoriaId.Value);

        if (temporadaId.HasValue)
            query = query.Where(t => t.TemporadaId == temporadaId.Value);

        return query
            .OrderBy(t => t.TemporadaId)
            .ThenBy(t => t.CategoriaHabitacionId)
            .ToListAsync(ct);
    }
}