using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Services.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CambiarFechas;

public sealed class CambiarFechasValidator : IValidator<CambiarFechasRequest>
{
    private readonly ISystemClock _clock;

    public CambiarFechasValidator(ISystemClock clock)
    {
        _clock = clock;
    }

    public Task<ValidacionResultado> ValidateAsync(
        CambiarFechasRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();

        if (request.ReservaId <= 0)
            errores.Add("ReservaId debe ser mayor a 0.");

        if (request.NuevaFechaEntrada == default)
            errores.Add("NuevaFechaEntrada es requerida.");

        if (request.NuevaFechaSalida == default)
            errores.Add("NuevaFechaSalida es requerida.");

        if (request.NuevaFechaEntrada >= request.NuevaFechaSalida)
            errores.Add("NuevaFechaEntrada debe ser anterior a NuevaFechaSalida.");

        if (request.NuevaFechaEntrada.Date < _clock.UtcNow.Date)
            errores.Add("NuevaFechaEntrada no puede ser en el pasado.");

        return Task.FromResult(errores.Count > 0
            ? ValidacionResultado.Failure(errores)
            : ValidacionResultado.Success());
    }
}