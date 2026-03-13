using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CambiarFechas;

public sealed record CambiarFechasRequest(
    int ReservaId,
    DateTime NuevaFechaEntrada,
    DateTime NuevaFechaSalida,
    AuditInfo AuditInfo);