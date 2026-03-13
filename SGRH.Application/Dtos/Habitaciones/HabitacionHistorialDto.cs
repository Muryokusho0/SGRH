using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Habitaciones;

public sealed record HabitacionHistorialDto(
    long HabitacionHistorialId,
    string EstadoHabitacion,
    string? Motivo,
    DateTime FechaInicio,
    DateTime? FechaFin);

