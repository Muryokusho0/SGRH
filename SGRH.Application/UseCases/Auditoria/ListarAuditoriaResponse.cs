using SGRH.Application.Dtos.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auditoria;

public sealed record ListarAuditoriaResponse(IReadOnlyList<AuditoriaEventoDto> Eventos);
