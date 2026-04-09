using SGRH.Application.Abstractions;
using SGRH.Application.UseCases.Auth.Register;
using System;
using Xunit;
using System.Threading.Tasks;

namespace SGRH.Tests.Application.Validators;

public sealed class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    private static RegisterRequest RequestValido(
        string? nationalId = "402-1234567-8",
        string? nombre = "Juan",
        string? apellido = "Pérez",
        string? telefono = "809-555-0000",
        string? email = "juan@email.com",
        string? password = "Password1!",
        string? confirmar = "Password1!") =>
        new(
            NationalId: nationalId!,
            NombreCliente: nombre!,
            ApellidoCliente: apellido!,
            Telefono: telefono!,
            Email: email!,
            Password: password!,
            ConfirmarPassword: confirmar!,
            AuditInfo: new(Guid.NewGuid(), "127.0.0.1", "TestAgent/1.0"));

    // ── Request válido ────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_RequestValido_RetornaSuccess()
    {
        var resultado = await _validator.ValidateAsync(RequestValido());
        Assert.True(resultado.IsValid);
        Assert.Empty(resultado.Errors);
    }

    // ── NationalId ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidarAsync_NationalIdVacio_RetornaError(string? val)
    {
        var resultado = await _validator.ValidateAsync(RequestValido(nationalId: val));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.Contains("NationalId"));
    }

    [Fact]
    public async Task ValidarAsync_NationalIdMasDe20Chars_RetornaError()
    {
        var resultado = await _validator.ValidateAsync(
            RequestValido(nationalId: new string('1', 21)));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.Contains("NationalId"));
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("sinArroba")]
    [InlineData("sin@dominio")]
    [InlineData("@sinlocal.com")]
    public async Task ValidarAsync_EmailFormato_Invalido_RetornaError(string email)
    {
        var resultado = await _validator.ValidateAsync(RequestValido(email: email));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.Contains("Email") && e.Contains("formato"));
    }

    [Fact]
    public async Task ValidarAsync_EmailMasDe100Chars_RetornaError()
    {
        var email = new string('a', 95) + "@b.com"; // > 100
        var resultado = await _validator.ValidateAsync(RequestValido(email: email));
        Assert.False(resultado.IsValid);
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("1234567")] // 7 chars — menos de 8
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidarAsync_PasswordInvalida_RetornaError(string? pass)
    {
        var resultado = await _validator.ValidateAsync(
            RequestValido(password: pass, confirmar: pass));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.Contains("Password"));
    }

    [Fact]
    public async Task ValidarAsync_PasswordsNoCoinciden_RetornaError()
    {
        var resultado = await _validator.ValidateAsync(
            RequestValido(password: "Password1!", confirmar: "OtraPassword!"));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.Contains("ConfirmarPassword") || e.Contains("coincid"));
    }

    // ── Múltiples errores ─────────────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_VariosCamposVacios_RetornaMultiplesErrores()
    {
        var request = new RegisterRequest(
            NationalId: "", NombreCliente: "", ApellidoCliente: "",
            Telefono: "", Email: "", Password: "",
            ConfirmarPassword: "",
            AuditInfo: new(Guid.NewGuid(), "127.0.0.1", "TestAgent/1.0"));

        var resultado = await _validator.ValidateAsync(request);

        Assert.False(resultado.IsValid);
        Assert.True(resultado.Errors.Count > 1,
            $"Se esperaban múltiples errores pero solo hubo {resultado.Errors.Count}");
    }
}