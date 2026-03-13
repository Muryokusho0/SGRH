using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.BloquearHabitacion;

public sealed record BloquearHabitacionRequest(
    int HabitacionId,
    string Motivo,
    AuditInfo AuditInfo);
