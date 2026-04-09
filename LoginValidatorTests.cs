using SGRH.Application.UseCases.Auth.Login;
using System;
using Xunit;
using System.Threading.Tasks;

namespace SGRH.Tests.Application.Validators;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    private static LoginRequest Req(
        string email = "juan@email.com",
        string password = "Password1!") =>
        new(email, password,
            new(Guid.NewGuid(), "127.0.0.1", "TestAgent/1.0"));

    [Fact]
    public async Task ValidarAsync_CredencialesValidas_RetornaSuccess()
    {
        var r = await _validator.ValidateAsync(Req());
        Assert.True(r.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidarAsync_EmailVacio_RetornaError(string? email)
    {
        var r = await _validator.ValidateAsync(Req(email: email!));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.ToLower().Contains("email"));
    }

    [Theory]
    [InlineData("noesunmail")]
    [InlineData("sin@dominio")]
    [InlineData("@sinlocal.com")]
    public async Task ValidarAsync_EmailSinFormato_RetornaError(string email)
    {
        var r = await _validator.ValidateAsync(Req(email: email));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors,
            e => e.ToLower().Contains("formato") || e.ToLower().Contains("email"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidarAsync_PasswordVacia_RetornaError(string? pass)
    {
        var r = await _validator.ValidateAsync(Req(password: pass!));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors,
            e => e.ToLower().Contains("contraseña") || e.ToLower().Contains("password"));
    }
}