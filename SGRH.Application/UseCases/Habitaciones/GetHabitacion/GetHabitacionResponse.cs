using SGRH.Application.Dtos.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.GetHabitacion;

public sealed record GetHabitacionResponse(HabitacionDto Habitacion);
