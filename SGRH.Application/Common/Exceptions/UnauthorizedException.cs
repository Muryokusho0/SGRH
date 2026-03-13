using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Common.Exceptions;
// Lanzada cuando las credenciales son inválidas o el usuario está inactivo.
// El middleware de la API la mapea a HTTP 401.
// El mensaje siempre es genérico para no revelar si el usuario existe.
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Credenciales inválidas.") { }

    public UnauthorizedException(string message)
        : base(message) { }
}
