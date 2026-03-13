using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CrearReserva;

public sealed record CrearReservaRequest(
    int ClienteId,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    AuditInfo AuditInfo);