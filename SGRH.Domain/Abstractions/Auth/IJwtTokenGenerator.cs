using SGRH.Domain.Entities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;

/// <summary>
/// Define el contrato para generar tokens JWT a partir de un usuario autenticado.
/// La implementación reside en la capa de infraestructura.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un token JWT firmado con la información del usuario indicado.
    /// </summary>
    /// <param name="usuario">Usuario autenticado cuya información se incluirá en los claims del token.</param>
    /// <returns>Un <see cref="TokenResult"/> con el token, fecha de expiración y datos del usuario.</returns>
    TokenResult Generate(Usuario usuario);
}