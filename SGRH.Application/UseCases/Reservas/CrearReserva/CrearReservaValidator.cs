using SGRH.Application.Abstractions;
using SGRH.Domain.Abstractions.Services.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Reservas.CrearReserva;

public sealed class CrearReservaValidator : IValidator<CrearReservaRequest>
{
    private readonly ISystemClock _clock;

    public CrearReservaValidator(ISystemClock clock)
    {
        _clock = clock;
    }

    public Task<ValidacionResultado> ValidateAsync(
        CrearReservaRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();

        if (request.ClienteId <= 0)
            errores.Add("ClienteId debe ser mayor a 0.");

        if (request.FechaEntrada == default)
            errores.Add("FechaEntrada es requerida.");

        if (request.FechaSalida == default)
            errores.Add("FechaSalida es requerida.");

        if (request.FechaEntrada >= request.FechaSalida)
            errores.Add("FechaEntrada debe ser anterior a FechaSalida.");

        if (request.FechaEntrada.Date < _clock.UtcNow.Date)
            errores.Add("FechaEntrada no puede ser en el pasado.");

        return Task.FromResult(errores.Count > 0
            ? ValidacionResultado.Failure(errores)
            : ValidacionResultado.Success());
    }
}
