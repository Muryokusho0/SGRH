using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.ListarHabitacionesDisponibles;

public sealed class ListarHabitacionesDisponiblesValidator
    : IValidator<ListarHabitacionesDisponiblesRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        ListarHabitacionesDisponiblesRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.FechaEntrada == default)
            errors.Add("La fecha de entrada es requerida.");

        if (request.FechaSalida == default)
            errors.Add("La fecha de salida es requerida.");

        if (request.FechaEntrada != default && request.FechaSalida != default)
        {
            if (request.FechaSalida <= request.FechaEntrada)
                errors.Add("La fecha de salida debe ser posterior a la fecha de entrada.");

            if (request.FechaEntrada < DateTime.UtcNow.Date)
                errors.Add("La fecha de entrada no puede ser en el pasado.");
        }

        if (request.CategoriaHabitacionId.HasValue && request.CategoriaHabitacionId.Value <= 0)
            errors.Add("La categoría de habitación no es válida.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
