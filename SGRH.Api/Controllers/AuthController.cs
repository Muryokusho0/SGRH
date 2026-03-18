using Microsoft.AspNetCore.Mvc;
using SGRH.Application.UseCases.Auth.Login;
using SGRH.Application.UseCases.Auth.Register;

namespace SGRH.Api.Controllers;

public sealed class AuthController : BaseApiController
{
    private readonly LoginUseCase _login;
    private readonly RegisterUseCase _register;

    public AuthController(LoginUseCase login, RegisterUseCase register)
    {
        _login = login;
        _register = register;
    }

    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginBody body, CancellationToken ct)
    {
        var request = new LoginRequest(body.Email, body.Password, BuildAuditInfo());
        var response = await _login.ExecuteAsync(request, ct);
        return Ok(response);
    }

    /// <summary>Registra un nuevo cliente y devuelve un token JWT.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterBody body, CancellationToken ct)
    {
        var request = new RegisterRequest(
            body.NationalId, body.NombreCliente, body.ApellidoCliente,
            body.Telefono, body.Email, body.Password, body.ConfirmarPassword,
            BuildAuditInfo());
        var response = await _register.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Login), response);
    }

    // ── Bodies ───────────────────────────────────────────────────────────

    public sealed record LoginBody(string Email, string Password);

    public sealed record RegisterBody(
        string NationalId, string NombreCliente, string ApellidoCliente,
        string Telefono, string Email, string Password, string ConfirmarPassword);
}