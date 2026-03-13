using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.AgregarServicio;

public sealed class AgregarServicioValidator : IValidator<AgregarServicioRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        AgregarServicioRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();

        if (request.ReservaId <= 0)
            errores.Add("ReservaId debe ser mayor a 0.");

        if (request.ServicioAdicionalId <= 0)
            errores.Add("ServicioAdicionalId debe ser mayor a 0.");

        if (request.Cantidad <= 0)
            errores.Add("Cantidad debe ser mayor a 0.");

        return Task.FromResult(errores.Count > 0
            ? ValidacionResultado.Failure(errores)
            : ValidacionResultado.Success());
    }
}
