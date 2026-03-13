using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SGRH.Application.Abstractions;

namespace SGRH.Application.UseCases.Auth.Register;

public sealed class RegisterValidator : IValidator<RegisterRequest>
{
    public Task<ValidacionResultado> ValidateAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(request.NationalId))
            errores.Add("NationalId es requerido.");
        else if (request.NationalId.Length > 20)
            errores.Add("NationalId no puede superar 20 caracteres.");

        if (string.IsNullOrWhiteSpace(request.NombreCliente))
            errores.Add("NombreCliente es requerido.");
        else if (request.NombreCliente.Length > 100)
            errores.Add("NombreCliente no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.ApellidoCliente))
            errores.Add("ApellidoCliente es requerido.");
        else if (request.ApellidoCliente.Length > 100)
            errores.Add("ApellidoCliente no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Telefono))
            errores.Add("Telefono es requerido.");
        else if (request.Telefono.Length > 20)
            errores.Add("Telefono no puede superar 20 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errores.Add("Email es requerido.");
        else if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errores.Add("Email no tiene un formato válido.");
        else if (request.Email.Length > 100)
            errores.Add("Email no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errores.Add("Password es requerido.");
        else if (request.Password.Length < 8)
            errores.Add("Password debe tener al menos 8 caracteres.");

        if (request.Password != request.ConfirmarPassword)
            errores.Add("Password y ConfirmarPassword no coinciden.");

        return Task.FromResult(errores.Count > 0
            ? ValidacionResultado.Failure(errores)
            : ValidacionResultado.Success());
    }
}