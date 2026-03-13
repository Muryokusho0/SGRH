using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.AgregarHabitacion;

public sealed record AgregarHabitacionRequest(
    int ReservaId,
    int HabitacionId,
    AuditInfo AuditInfo);
