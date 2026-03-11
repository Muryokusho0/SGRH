using SGRH.Domain.Entities.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services;

public interface IAuditoriaService
{
    Task RegistrarAsync(AuditoriaEvento evento, CancellationToken ct = default);
}
