using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("VALIDATION_ERROR", "Hay errores de validación.")
    {
        Errors = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
                 ?? new List<string>();
    }
}
