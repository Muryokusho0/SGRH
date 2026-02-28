using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IAuditoriaRepository : IRepository<AuditoriaEvento, int>
{
    Task AddDetalleAsync(AuditoriaEventoDetalle detalle, CancellationToken ct = default);
}
