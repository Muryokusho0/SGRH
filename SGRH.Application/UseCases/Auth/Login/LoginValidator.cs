using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Auth.Login;

public sealed class LoginValidator : IValidator<LoginRequest>
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public Task<ValidacionResultado> ValidateAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("El email es requerido.");
        else if (!EmailRegex.IsMatch(request.Email))
            errors.Add("El email no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("La contraseña es requerida.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
