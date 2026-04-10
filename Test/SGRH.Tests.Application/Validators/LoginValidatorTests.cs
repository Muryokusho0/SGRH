using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGRH.Application.UseCases.Auth.Login;

namespace SGRH.Tests.Application.Validators;

[TestClass]
public sealed class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    private static LoginRequest Req(
        string email = "juan@email.com",
        string password = "Password1!") =>
        new(email, password,
            new(Guid.NewGuid(), "127.0.0.1", "Test/1.0"));

    [TestMethod]
    public async Task ValidarAsync_CredencialesValidas_RetornaSuccess()
    {
        var r = await _validator.ValidateAsync(Req(), TestContext.CancellationToken);
        Assert.IsTrue(r.IsValid);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ValidarAsync_EmailVacio_RetornaError(string email)
    {
        var r = await _validator.ValidateAsync(Req(email: email), TestContext.CancellationToken);
        Assert.IsFalse(r.IsValid);
        Assert.Contains(e => e.Contains("email", StringComparison.CurrentCultureIgnoreCase), r.Errors);
    }

    [TestMethod]
    [DataRow("noesunmail")]
    [DataRow("sin@dominio")]
    [DataRow("@sinlocal.com")]
    public async Task ValidarAsync_EmailSinFormato_RetornaError(string email)
    {
        var r = await _validator.ValidateAsync(Req(email: email), TestContext.CancellationToken);
        Assert.IsFalse(r.IsValid);
        Assert.Contains(e =>
            e.Contains("formato", StringComparison.CurrentCultureIgnoreCase) || e.Contains("email", StringComparison.CurrentCultureIgnoreCase), r.Errors);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ValidarAsync_PasswordVacia_RetornaError(string pass)
    {
        var r = await _validator.ValidateAsync(Req(password: pass), TestContext.CancellationToken);
        Assert.IsFalse(r.IsValid);
        Assert.Contains(e =>
            e.Contains("contraseña", StringComparison.CurrentCultureIgnoreCase) || e.Contains("password", StringComparison.CurrentCultureIgnoreCase), r.Errors);
    }

    public TestContext TestContext { get; set; }
}
