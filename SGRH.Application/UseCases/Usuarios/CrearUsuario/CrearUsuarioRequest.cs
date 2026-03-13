using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Usuarios.CrearUsuario;
// Solo crea ADMIN o RECEPCIONISTA — nunca CLIENTE.
// Los clientes se crean únicamente a través de Register.
public sealed record CrearUsuarioRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string Rol,         
    AuditInfo AuditInfo);
