using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Application.UseCases.Auth.Register;

namespace SGRH.Tests.Application.Validators;

[TestClass]
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
            AuditInfo: new(Guid.NewGuid(), "127.0.0.1", "Test/1.0"));

    [TestMethod]
    public async Task ValidarAsync_RequestValido_RetornaSuccess()
    {
        var r = await _validator.ValidateAsync(RequestValido());
        Assert.IsTrue(r.IsValid);
        Assert.AreEqual(0, r.Errors.Count);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ValidarAsync_NationalIdVacio_RetornaError(string? val)
    {
        var r = await _validator.ValidateAsync(RequestValido(nationalId: val));
        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Any(e => e.Contains("NationalId")));
    }

    [TestMethod]
    public async Task ValidarAsync_NationalIdMasDe20Chars_RetornaError()
    {
        var r = await _validator.ValidateAsync(
            RequestValido(nationalId: new string('1', 21)));
        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Any(e => e.Contains("NationalId")));
    }

    [TestMethod]
    [DataRow("sinArroba")]
    [DataRow("sin@dominio")]
    [DataRow("@sinlocal.com")]
    public async Task ValidarAsync_EmailFormatoInvalido_RetornaError(string email)
    {
        var r = await _validator.ValidateAsync(RequestValido(email: email));
        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Any(e =>
            e.Contains("Email") && e.ToLower().Contains("formato")));
    }

    [TestMethod]
    [DataRow("1234567")] // 7 chars
    [DataRow("")]
    public async Task ValidarAsync_PasswordInvalida_RetornaError(string pass)
    {
        var r = await _validator.ValidateAsync(
            RequestValido(password: pass, confirmar: pass));
        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Any(e => e.Contains("Password")));
    }

    [TestMethod]
    public async Task ValidarAsync_PasswordsNoCoinciden_RetornaError()
    {
        var r = await _validator.ValidateAsync(
            RequestValido(password: "Password1!", confirmar: "OtraPass!"));
        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Any(e =>
            e.Contains("ConfirmarPassword") || e.ToLower().Contains("coincid")));
    }

    [TestMethod]
    public async Task ValidarAsync_VariosCamposVacios_RetornaMultiplesErrores()
    {
        var request = new RegisterRequest(
            NationalId: "", NombreCliente: "", ApellidoCliente: "",
            Telefono: "", Email: "", Password: "", ConfirmarPassword: "",
            AuditInfo: new(Guid.NewGuid(), "127.0.0.1", "Test/1.0"));

        var r = await _validator.ValidateAsync(request);

        Assert.IsFalse(r.IsValid);
        Assert.IsTrue(r.Errors.Count > 1,
            $"Se esperaban múltiples errores, solo hubo {r.Errors.Count}");
    }
}
