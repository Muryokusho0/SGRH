using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.AgregarServicio;

public sealed record AgregarServicioRequest(
    int ReservaId,
    int ServicioAdicionalId,
    int Cantidad,
    AuditInfo AuditInfo);
