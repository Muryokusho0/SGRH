using SGRH.Application.Dtos.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Clientes.GetCliente;

public sealed record GetClienteResponse(ClienteDto Cliente);
