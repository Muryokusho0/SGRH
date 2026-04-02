using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;

/// <summary>
/// Resultado de la generación de un token JWT. Contiene el token firmado
/// y los metadatos asociados al usuario autenticado.
/// </summary>
/// <param name="Token">Cadena del token JWT firmado.</param>
/// <param name="ExpiresAtUtc">Fecha y hora en UTC en que el token expira.</param>
/// <param name="UsuarioId">Identificador del usuario al que pertenece el token.</param>
/// <param name="Username">Nombre de usuario incluido en los claims del token.</param>
/// <param name="Rol">Rol del usuario incluido en los claims del token.</param>
public sealed record TokenResult(
    string Token,
    DateTime ExpiresAtUtc,
    int UsuarioId,
    string Username,
    string Rol);