using SGRH.Application.Abstractions;
using SGRH.Domain.Enums;

namespace SGRH.Application.UseCases.Habitaciones.CambiarEstadoHabitacion;

public sealed class CambiarEstadoHabitacionValidator
    : IValidator<CambiarEstadoHabitacionRequest>
{
    private static readonly HashSet<string> EstadosValidos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(EstadoHabitacion.Disponible),
            nameof(EstadoHabitacion.Ocupada),
            nameof(EstadoHabitacion.Mantenimiento),
            nameof(EstadoHabitacion.Limpieza)
        };

    private static readonly HashSet<string> EstadosConMotivoRequerido =
        new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(EstadoHabitacion.Mantenimiento),
            nameof(EstadoHabitacion.Limpieza)
        };

    public Task<ValidacionResultado> ValidateAsync(
        CambiarEstadoHabitacionRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.HabitacionId <= 0)
            errors.Add("El HabitacionId no es válido.");

        if (string.IsNullOrWhiteSpace(request.NuevoEstado))
            errors.Add("El nuevo estado es requerido.");
        else if (!EstadosValidos.Contains(request.NuevoEstado))
            errors.Add($"El estado '{request.NuevoEstado}' no es válido. " +
                       "Use: Disponible, Ocupada, Mantenimiento o Limpieza.");
        else
        {
            if (EstadosConMotivoRequerido.Contains(request.NuevoEstado)
                && string.IsNullOrWhiteSpace(request.Motivo))
                errors.Add($"El motivo es requerido para el estado {request.NuevoEstado}.");

            if (!EstadosConMotivoRequerido.Contains(request.NuevoEstado)
                && !string.IsNullOrWhiteSpace(request.Motivo))
                errors.Add($"El estado {request.NuevoEstado} no debe llevar motivo.");
        }

        if (!string.IsNullOrWhiteSpace(request.Motivo) && request.Motivo.Length > 255)
            errors.Add("El motivo no puede superar 255 caracteres.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}