using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.QuitarHabitacion;

public sealed record QuitarHabitacionRequest(
    int ReservaId,
    int HabitacionId,
    AuditInfo AuditInfo);
