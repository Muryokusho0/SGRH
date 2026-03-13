using SGRH.Application.Dtos.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.GetHistorialHabitacion;

public sealed record GetHistorialHabitacionResponse(
    int HabitacionId,
    int NumeroHabitacion,
    int Piso,
    IReadOnlyList<HabitacionHistorialDto> Historial);
