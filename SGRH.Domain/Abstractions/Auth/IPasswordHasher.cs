using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;

/// <summary>
/// Define el contrato para el hash y verificación de contraseñas de usuario.
/// La implementación reside en la capa de infraestructura y utiliza un algoritmo seguro (por ejemplo, BCrypt).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera el hash seguro de una contraseña en texto plano.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a hashear.</param>
    /// <returns>Cadena con el hash resultante listo para almacenarse en la base de datos.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con el hash almacenado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano proporcionada por el usuario.</param>
    /// <param name="hash">Hash almacenado en la base de datos.</param>
    /// <returns><c>true</c> si la contraseña coincide con el hash; de lo contrario, <c>false</c>.</returns>
    bool Verify(string password, string hash);
}