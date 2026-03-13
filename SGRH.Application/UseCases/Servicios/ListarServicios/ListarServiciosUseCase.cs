using SGRH.Application.Dtos.Servicios;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Servicios.ListarServicios;

public sealed class ListarServiciosUseCase
{
    private readonly IServicioAdicionalRepository _servicios;

    public ListarServiciosUseCase(IServicioAdicionalRepository servicios)
    {
        _servicios = servicios;
    }

    public async Task<ListarServiciosResponse> ExecuteAsync(
        string? nombre = null,
        CancellationToken ct = default)
    {
        var servicios = await _servicios.BuscarAsync(nombre, ct);

        var dtos = servicios.Select(s => s.ToDto()).ToList();

        return new ListarServiciosResponse(dtos);
    }
}