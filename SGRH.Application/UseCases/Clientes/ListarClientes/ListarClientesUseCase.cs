using SGRH.Application.Dtos.Clientes;
using SGRH.Application.Mappers;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Application.UseCases.Clientes.ListarClientes;

public sealed class ListarClientesUseCase
{
    private readonly IClienteRepository _clientes;

    public ListarClientesUseCase(IClienteRepository clientes)
    {
        _clientes = clientes;
    }

    public async Task<ListarClientesResponse> ExecuteAsync(
        string? nombre = null,
        string? email = null,
        string? nationalId = null,
        CancellationToken ct = default)
    {
        var clientes = await _clientes.BuscarAsync(nombre, email, nationalId, ct);

        var dtos = clientes.Select(c => c.ToDto()).ToList();

        return new ListarClientesResponse(dtos);
    }
}