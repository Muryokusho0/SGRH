using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using SGRH.Application.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Servicios.GetServicio;

public sealed class GetServicioUseCase
{
    private readonly IServicioAdicionalRepository _servicios;

    public GetServicioUseCase(IServicioAdicionalRepository servicios)
    {
        _servicios = servicios;
    }

    public async Task<GetServicioResponse> ExecuteAsync(
        int servicioId, CancellationToken ct = default)
    {
        var servicio = await _servicios.GetByIdAsync(servicioId, ct)
            ?? throw new NotFoundException("ServicioAdicional", servicioId.ToString());

        return new GetServicioResponse(servicio.ToDto());
    }
}
