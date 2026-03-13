using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Tarifas.CrearTarifa;

public sealed class CrearTarifaValidator : IValidator<CrearTarifaRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        CrearTarifaRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.CategoriaHabitacionId <= 0)
            errors.Add("La categoría de habitación no es válida.");

        if (request.TemporadaId <= 0)
            errors.Add("La temporada no es válida.");

        if (request.PrecioNoche <= 0)
            errors.Add("El precio por noche debe ser mayor a cero.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
