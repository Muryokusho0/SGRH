using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.QuitarServicio;

public sealed record QuitarServicioRequest(
    int ReservaId,
    int ServicioAdicionalId,
    AuditInfo AuditInfo);
