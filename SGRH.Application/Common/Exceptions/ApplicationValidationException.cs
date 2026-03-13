using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Common.Exceptions;
// Lanzada cuando IValidator<TRequest>.ValidateAsync() devuelve fallos.
// El middleware de la API la mapea a HTTP 400 con la lista de errores.
public sealed class ApplicationValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ApplicationValidationException(IEnumerable<string> errors)
        : base("Hay errores de validación.")
    {
        Errors = errors.ToList();
    }
}
