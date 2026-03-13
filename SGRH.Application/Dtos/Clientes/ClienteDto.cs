using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Dtos.Clientes;

public sealed record ClienteDto(
    int ClienteId,
    string NationalId,
    string NombreCliente,
    string ApellidoCliente,
    string Email,
    string Telefono);
