using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Temporadas;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class TemporadaRepositoryEF
    : Repository<Temporada, int>, ITemporadaRepository
{
    public TemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    /// <summary>
    /// Devuelve la temporada vigente para una fecha dada.
    /// FechaFin es exclusiva: la temporada va de FechaInicio (inclusive) a FechaFin (exclusive).
    /// </summary>
    public Task<Temporada?> GetByFechaAsync(
        DateTime fecha, CancellationToken ct = default)
        => Db.Temporadas
            .AsNoTracking()
            .Where(t => fecha >= t.FechaInicio && fecha < t.FechaFin)
            .OrderByDescending(t => t.FechaInicio)
            .FirstOrDefaultAsync(ct);
}