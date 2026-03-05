using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Servicios;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class ServicioTemporadaRepositoryEF
    : Repository<ServicioTemporada, (int ServicioId, int TemporadaId)>, IServicioTemporadaRepository
{
    public ServicioTemporadaRepositoryEF(SGRHDbContext db) : base(db) { }

    public override Task<ServicioTemporada?> GetByIdAsync((int ServicioId, int TemporadaId) id, CancellationToken ct = default)
        => Db.ServicioTemporadas.FindAsync([id.ServicioId, id.TemporadaId], ct).AsTask();
}