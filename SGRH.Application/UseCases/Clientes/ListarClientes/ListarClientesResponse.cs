using SGRH.Application.Dtos.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Clientes.ListarClientes;

public sealed record ListarClientesResponse(IReadOnlyList<ClienteDto> Clientes);
