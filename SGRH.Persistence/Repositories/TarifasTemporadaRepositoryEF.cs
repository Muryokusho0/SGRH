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

public sealed class TarifaTemporadaRepositoryEF
    : Repository<TarifaTemporada, int>, ITarifaTemporadaRepository
{
    public TarifaTemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<TarifaTemporada?> GetTarifaAsync(int categoriaId, int temporadaId, CancellationToken ct = default)
        => Db.TarifasTemporada
            .FirstOrDefaultAsync(t => t.CategoriaHabitacionId == categoriaId && t.TemporadaId == temporadaId, ct);
}
