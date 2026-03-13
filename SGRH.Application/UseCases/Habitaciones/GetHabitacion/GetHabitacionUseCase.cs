using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.GetHabitacion;

public sealed class GetHabitacionUseCase
{
    private readonly IHabitacionRepository _habitaciones;
    private readonly ICategoriaHabitacionRepository _categorias;

    public GetHabitacionUseCase(
        IHabitacionRepository habitaciones,
        ICategoriaHabitacionRepository categorias)
    {
        _habitaciones = habitaciones;
        _categorias = categorias;
    }

    public async Task<GetHabitacionResponse> ExecuteAsync(
        int habitacionId, CancellationToken ct = default)
    {
        var habitacion = await _habitaciones.GetByIdAsync(habitacionId, ct)
            ?? throw new NotFoundException("Habitacion", habitacionId.ToString());

        var categoria = await _categorias.GetByIdAsync(habitacion.CategoriaHabitacionId, ct);
        var nombreCategoria = categoria?.NombreCategoria ?? string.Empty;

        return new GetHabitacionResponse(
            HabitacionMapper.ToDto(habitacion, nombreCategoria));
    }
}
