using SGRH.Application.Dtos.Categorias;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;

public static class CategoriaMapper
{
    public static CategoriaDto ToDto(this CategoriaHabitacion categoria) =>
        new(
            CategoriaHabitacionId: categoria.CategoriaHabitacionId,
            NombreCategoria: categoria.NombreCategoria,
            Capacidad: categoria.Capacidad,
            Descripcion: categoria.Descripcion,
            PrecioBase: categoria.PrecioBase);

    public static IReadOnlyList<CategoriaDto> ToDtoList(
        this IEnumerable<CategoriaHabitacion> categorias) =>
        categorias.Select(ToDto).ToList();
}
