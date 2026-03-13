using SGRH.Domain.Abstractions.Repositories;
using SGRH.Application.Mappers;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.GetHistorialHabitacion;

// Consulta el historial completo de cambios de estado de una habitación.
// Diferente a GetHabitacion — aquí se devuelven TODOS los registros del historial
// en orden cronológico descendente, no solo el estado actual.
public sealed class GetHistorialHabitacionUseCase
{
    private readonly IHabitacionRepository _habitaciones;

    public GetHistorialHabitacionUseCase(IHabitacionRepository habitaciones)
    {
        _habitaciones = habitaciones;
    }

    public async Task<GetHistorialHabitacionResponse> ExecuteAsync(
        int habitacionId, CancellationToken ct = default)
    {
        // GetByIdWithHistorialAsync carga la habitación con toda la colección Historial
        var habitacion = await _habitaciones.GetByIdWithHistorialAsync(habitacionId, ct)
            ?? throw new NotFoundException("Habitacion", habitacionId.ToString());

        // Ordenar descendente — el estado más reciente primero
        var historialOrdenado = habitacion.Historial
            .OrderByDescending(h => h.FechaInicio)
            .Select(h => h.ToDto())
            .ToList();

        return new GetHistorialHabitacionResponse(
            HabitacionId: habitacion.HabitacionId,
            NumeroHabitacion: habitacion.NumeroHabitacion,
            Piso: habitacion.Piso,
            Historial: historialOrdenado);
    }
}
