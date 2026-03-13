using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.ModificarCategoria;

public sealed class ModificarCategoriaValidator : IValidator<ModificarCategoriaRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        ModificarCategoriaRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.CategoriaHabitacionId <= 0)
            errors.Add("El CategoriaHabitacionId no es válido.");

        if (string.IsNullOrWhiteSpace(request.NombreCategoria))
            errors.Add("El nombre de la categoría es requerido.");
        else if (request.NombreCategoria.Length > 50)
            errors.Add("El nombre no puede superar 50 caracteres.");

        if (request.Capacidad <= 0)
            errors.Add("La capacidad debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(request.Descripcion))
            errors.Add("La descripción es requerida.");
        else if (request.Descripcion.Length > 255)
            errors.Add("La descripción no puede superar 255 caracteres.");

        if (request.PrecioBase <= 0)
            errors.Add("El precio base debe ser mayor a cero.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
