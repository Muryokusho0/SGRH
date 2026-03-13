using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Abstractions;

public sealed class ValidacionResultado
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    private ValidacionResultado(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidacionResultado Success()
        => new(true, []);

    public static ValidacionResultado Failure(IEnumerable<string> errors)
        => new(false, errors.ToList());

    public static ValidacionResultado Combine(IEnumerable<ValidacionResultado> results)
    {
        var errors = results.SelectMany(r => r.Errors).ToList();
        return errors.Count > 0 ? Failure(errors) : Success();
    }
}