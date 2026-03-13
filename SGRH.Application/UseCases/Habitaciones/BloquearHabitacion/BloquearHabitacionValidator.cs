using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.BloquearHabitacion;

public sealed class BloquearHabitacionValidator : IValidator<BloquearHabitacionRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        BloquearHabitacionRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.HabitacionId <= 0)
            errors.Add("El HabitacionId no es válido.");

        if (string.IsNullOrWhiteSpace(request.Motivo))
            errors.Add("El motivo del bloqueo es requerido.");
        else if (request.Motivo.Length > 255)
            errors.Add("El motivo no puede superar 255 caracteres.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}

