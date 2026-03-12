using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;
// Resultado de generar un JWT. Contiene el token y sus metadatos.
public sealed record TokenResult(
    string Token,
    DateTime ExpiresAtUtc,
    int UsuarioId,
    string Username,
    string Rol);