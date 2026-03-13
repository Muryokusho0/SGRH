using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Exceptions;
using SGRH.Application.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Clientes.GetCliente;
// Sin Request ni Validator — solo recibe el ID a consultar.
public sealed class GetClienteUseCase
{
    private readonly IClienteRepository _clientes;

    public GetClienteUseCase(IClienteRepository clientes)
    {
        _clientes = clientes;
    }

    public async Task<GetClienteResponse> ExecuteAsync(
        int clienteId, CancellationToken ct = default)
    {
        var cliente = await _clientes.GetByIdAsync(clienteId, ct)
            ?? throw new NotFoundException("Cliente", clienteId.ToString());

        return new GetClienteResponse(cliente.ToDto());
    }
}
