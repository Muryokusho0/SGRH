using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Auditoria;

namespace SGRH.Domain.Abstractions.Persistence;

public interface IAuditoriaRepository
{
    Task AddAsync(AuditoriaEvento evento, CancellationToken ct = default);
}
