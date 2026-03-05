using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class AuditoriaRepositoryEF : Repository<AuditoriaEvento, int>, IAuditoriaRepository
{
    public AuditoriaRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task AddDetalleAsync(AuditoriaEventoDetalle detalle, CancellationToken ct = default)
        => Db.AuditoriaEventoDetalles.AddAsync(detalle, ct).AsTask();
}
