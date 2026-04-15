using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Auth;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Desktop.Services.Auth;

public interface IAuthService
{
    Task LoginAsync(LoginRequest request);
    void Logout();
}

public sealed class AuthService : ApiServiceBase, IAuthService
{
    private readonly TokenStorageService _sesion;

    public AuthService(
        IHttpClientFactory factory,
        TokenStorageService sesion,
        ILogger<AuthService> logger)
        : base(factory, sesion, logger)
    {
        _sesion = sesion;
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var resp = await LoggedAsync("POST", "api/auth/login",
            () => CrearCliente().PostAsJsonAsync("api/auth/login", request));

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                await ApiErrorHelper.LeerErrorAsync(resp));

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString()!;
        var rol = json.GetProperty("rol").GetString()!;
        var username = json.GetProperty("username").GetString()!;

        if (rol is not "ADMIN" and not "RECEPCIONISTA")
            throw new InvalidOperationException(
                "Esta aplicación es solo para Administradores y Recepcionistas.");

        _sesion.Guardar(token, rol, username);
    }

    public void Logout() => _sesion.LimpiarSesion();
}