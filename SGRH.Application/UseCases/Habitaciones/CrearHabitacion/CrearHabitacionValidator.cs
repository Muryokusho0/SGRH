using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Habitaciones.CrearHabitacion;

public sealed class CrearHabitacionValidator : IValidator<CrearHabitacionRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        CrearHabitacionRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();
 
        if (request.CategoriaHabitacionId <= 0)
            errors.Add("La categoría de habitación no es válida.");
 
        if (request.NumeroHabitacion <= 0)
            errors.Add("El número de habitación debe ser mayor a cero.");
 
        if (request.NumeroPiso <= 0)
            errors.Add("El número de piso debe ser mayor a cero.");
 
        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
