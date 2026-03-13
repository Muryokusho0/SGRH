using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.CrearHabitacion;

public sealed record CrearHabitacionRequest(
    int NumeroPiso,
    int NumeroHabitacion,
    int CategoriaHabitacionId,
    AuditInfo AuditInfo);