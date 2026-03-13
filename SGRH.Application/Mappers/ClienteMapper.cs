using SGRH.Application.Dtos.Clientes;
using SGRH.Domain.Entities.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Mappers;
public static class ClienteMapper
{
    public static ClienteDto ToDto(this Cliente cliente) =>
        new(
            ClienteId: cliente.ClienteId,
            NationalId: cliente.NationalId,
            NombreCliente: cliente.NombreCliente,
            ApellidoCliente: cliente.ApellidoCliente,
            Email: cliente.Email,
            Telefono: cliente.Telefono);

    public static IReadOnlyList<ClienteDto> ToDtoList(
        this IEnumerable<Cliente> clientes) =>
        clientes.Select(ToDto).ToList();
}