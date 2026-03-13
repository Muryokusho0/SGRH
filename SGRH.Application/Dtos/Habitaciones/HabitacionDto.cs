using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Dtos.Categorias;

namespace SGRH.Application.Dtos.Habitaciones;

public sealed record HabitacionDto(
    int HabitacionId,
    int CategoriaHabitacionId,
    string NombreCategoria,
    int NumeroHabitacion,
    int Piso,
    string EstadoActual,
    IReadOnlyList<HabitacionHistorialDto> Historial);
