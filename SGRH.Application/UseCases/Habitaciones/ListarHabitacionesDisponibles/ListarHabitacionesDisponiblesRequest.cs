using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.ListarHabitacionesDisponibles;

// Consulta de disponibilidad por rango de fechas.
// CategoriaHabitacionId es opcional — si se omite devuelve todas las categorías.
public sealed record ListarHabitacionesDisponiblesRequest(
    DateTime FechaEntrada,
    DateTime FechaSalida,
    int? CategoriaHabitacionId = null);
