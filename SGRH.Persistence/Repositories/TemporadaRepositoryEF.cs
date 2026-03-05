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

public sealed class TemporadaRepositoryEF : Repository<Temporada, int>, ITemporadaRepository
{
    public TemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Temporada?> GetByFechaAsync(DateTime fecha, CancellationToken ct = default)
        => Db.Temporadas
            .Where(t => fecha >= t.FechaInicio && fecha <= t.FechaFin)
            .OrderByDescending(t => t.FechaInicio)
            .FirstOrDefaultAsync(ct);
}