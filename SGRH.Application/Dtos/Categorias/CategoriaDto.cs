using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Categorias;

public sealed record CategoriaDto(
    int CategoriaHabitacionId,
    string NombreCategoria,
    int Capacidad,
    string Descripcion,
    decimal PrecioBase);
