using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Clientes.ModificarCliente;

public sealed class ModificarClienteValidator : IValidator<ModificarClienteRequest>
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public Task<ValidacionResultado> ValidateAsync(
        ModificarClienteRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();

        if (request.ClienteId <= 0)
            errors.Add("El ClienteId no es válido.");

        if (string.IsNullOrWhiteSpace(request.NombreCliente))
            errors.Add("El nombre es requerido.");
        else if (request.NombreCliente.Length > 100)
            errors.Add("El nombre no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.ApellidoCliente))
            errors.Add("El apellido es requerido.");
        else if (request.ApellidoCliente.Length > 100)
            errors.Add("El apellido no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("El email es requerido.");
        else if (request.Email.Length > 100)
            errors.Add("El email no puede superar 100 caracteres.");
        else if (!EmailRegex.IsMatch(request.Email))
            errors.Add("El email no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(request.Telefono))
            errors.Add("El teléfono es requerido.");
        else if (request.Telefono.Length > 20)
            errors.Add("El teléfono no puede superar 20 caracteres.");

        return Task.FromResult(
            errors.Count > 0
                ? ValidacionResultado.Failure(errors)
                : ValidacionResultado.Success());
    }
}
