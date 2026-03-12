using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Auth;

public interface IPasswordHasher
{
    // Genera el hash de una contraseña en texto plano.
    string Hash(string password);

    // Verifica si una contraseña en texto plano coincide con el hash almacenado.
    bool Verify(string password, string hash);
}