using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Servicios.ListarServiciosPorCategoria;
// DTO local — incluye precio específico para la categoría solicitada.
// Distinto a ServicioDto que no tiene precio por categoría.
public sealed record ServicioPorCategoriaDto(
    int ServicioAdicionalId,
    string NombreServicio,
    string Descripcion,
    decimal PrecioPorCategoria);

public sealed record ListarServiciosPorCategoriaResponse(
    int CategoriaHabitacionId,
    IReadOnlyList<ServicioPorCategoriaDto> Servicios);