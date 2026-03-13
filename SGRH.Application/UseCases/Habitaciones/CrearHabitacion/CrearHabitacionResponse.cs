using SGRH.Application.Dtos.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.CrearHabitacion;

public sealed record CrearHabitacionResponse(HabitacionDto Habitacion);
