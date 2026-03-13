using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.AgregarHabitacion;

public sealed class AgregarHabitacionValidator : IValidator<AgregarHabitacionRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        AgregarHabitacionRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();

        if (request.ReservaId <= 0)
            errores.Add("ReservaId debe ser mayor a 0.");

        if (request.HabitacionId <= 0)
            errores.Add("HabitacionId debe ser mayor a 0.");

        return Task.FromResult(errores.Count > 0
            ? ValidacionResultado.Failure(errores)
            : ValidacionResultado.Success());
    }
}
