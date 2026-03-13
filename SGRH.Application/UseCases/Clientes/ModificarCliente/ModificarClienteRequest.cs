using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Clientes.ModificarCliente;

// NationalId no se modifica — es el identificador permanente del cliente.
// ClienteId viene por ruta en el Controller, no en el body.
public sealed record ModificarClienteRequest(
    int ClienteId,
    string NombreCliente,
    string ApellidoCliente,
    string Email,
    string Telefono,
    AuditInfo AuditInfo);
