using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Usuarios.CrearUsuario;

public sealed class CrearUsuarioValidator : IValidator<CrearUsuarioRequest>
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static readonly HashSet<string> RolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "ADMIN", "RECEPCIONISTA" };

    public Task<ValidacionResultado> ValidateAsync(
        CrearUsuarioRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("El email es requerido.");
        else if (request.Email.Length > 100)
            errors.Add("El email no puede superar 100 caracteres.");
        else if (!EmailRegex.IsMatch(request.Email))
            errors.Add("El email no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("La contraseña es requerida.");
        else if (request.Password.Length < 8)
            errors.Add("La contraseña debe tener al menos 8 caracteres.");

        if (request.Password != request.ConfirmPassword)
            errors.Add("Las contraseñas no coinciden.");

        if (string.IsNullOrWhiteSpace(request.Rol))
            errors.Add("El rol es requerido.");
        else if (!RolesPermitidos.Contains(request.Rol))
            errors.Add("El rol debe ser ADMIN o RECEPCIONISTA.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
