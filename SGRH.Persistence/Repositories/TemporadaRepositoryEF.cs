using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class TemporadaRepositoryEF
    : Repository<Temporada, int>, ITemporadaRepository
{
    public TemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    // FechaFin es exclusiva: [FechaInicio, FechaFin)
    public Task<Temporada?> GetByFechaAsync(
        DateTime fecha, CancellationToken ct = default)
        => Db.Temporadas
            .AsNoTracking()
            .Where(t => fecha >= t.FechaInicio && fecha < t.FechaFin)
            .OrderByDescending(t => t.FechaInicio)
            .FirstOrDefaultAsync(ct);

    public Task<bool> ExisteSolapamientoAsync(
        DateTime fechaInicio, DateTime fechaFin, int? excludeId,
        CancellationToken ct = default)
        => Db.Temporadas
            .AnyAsync(t =>
                (excludeId == null || t.TemporadaId != excludeId) &&
                t.FechaInicio < fechaFin &&
                t.FechaFin > fechaInicio, ct);

    public Task<List<Temporada>> BuscarAsync(
        string? nombre, DateTime? fechaDesde, DateTime? fechaHasta,
        CancellationToken ct = default)
    {
        var query = Db.Temporadas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(t => t.NombreTemporada.Contains(nombre));

        if (fechaDesde.HasValue)
            query = query.Where(t => t.FechaFin > fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(t => t.FechaInicio < fechaHasta.Value);

        return query.OrderBy(t => t.FechaInicio).ToListAsync(ct);
    }
}