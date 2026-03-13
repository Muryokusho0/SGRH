using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Servicios.CrearServicio;

public sealed class CrearServicioValidator : IValidator<CrearServicioRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        CrearServicioRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.NombreServicio))
            errors.Add("El nombre del servicio es requerido.");
        else if (request.NombreServicio.Length > 100)
            errors.Add("El nombre no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Descripcion))
            errors.Add("La descripción es requerida.");
        else if (request.Descripcion.Length > 255)
            errors.Add("La descripción no puede superar 255 caracteres.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
