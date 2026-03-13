using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Temporadas.CrearTemporada;

public sealed class CrearTemporadaValidator : IValidator<CrearTemporadaRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        CrearTemporadaRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.NombreTemporada))
            errors.Add("El nombre de la temporada es requerido.");
        else if (request.NombreTemporada.Length > 50)
            errors.Add("El nombre no puede superar 50 caracteres.");

        if (request.FechaInicio == default)
            errors.Add("La fecha de inicio es requerida.");

        if (request.FechaFin == default)
            errors.Add("La fecha fin es requerida.");

        if (request.FechaInicio != default && request.FechaFin != default
            && request.FechaFin <= request.FechaInicio)
            errors.Add("La fecha fin debe ser posterior a la fecha de inicio.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
