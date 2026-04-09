using Microsoft.AspNetCore.Components.Authorization;
using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models;
using SGRH.Web.Models.Auth;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGRH.Web.Services.Auth;

public sealed class AuthService : ApiServiceBase, IAuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage,
        AuthenticationStateProvider authStateProvider)
        : base(factory, tokenStorage)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<SesionViewModel> LoginAsync(
        LoginViewModel model, CancellationToken ct = default)
    {
        // Login no requiere token — CrearCliente() no añade header si no hay token
        var client = CrearCliente();
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(
                "api/auth/login",
                new { Email = model.Email, Password = model.Password },
                ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex));
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await LeerErrorAsync(response, ct);
            throw new InvalidOperationException(ApiErrorHelper.TraducirError(error));
        }

        return await ProcesarTokenResponseAsync(response, ct);
    }

    public async Task<SesionViewModel> RegisterAsync(
        RegisterViewModel model, CancellationToken ct = default)
    {
        if (model.Password != model.ConfirmarPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden.");

        var client = CrearCliente();
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(
                "api/auth/register",
                new
                {
                    NationalId = model.NationalId,
                    NombreCliente = model.NombreCliente,
                    ApellidoCliente = model.ApellidoCliente,
                    Telefono = model.Telefono,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmarPassword = model.ConfirmarPassword
                },
                ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex));
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await LeerErrorAsync(response, ct);
            throw new InvalidOperationException(ApiErrorHelper.TraducirError(error));
        }

        return await ProcesarTokenResponseAsync(response, ct);
    }

    public void Logout()
    {
        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            jwtProvider.NotificarLogout();
    }

    private async Task<SesionViewModel> ProcesarTokenResponseAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var tokenNode = doc.RootElement.GetProperty("token");

        var tokenString = tokenNode.GetProperty("token").GetString()!;
        var username = tokenNode.GetProperty("username").GetString()!;
        var rol = tokenNode.GetProperty("rol").GetString()!;
        var expiresAt = tokenNode.GetProperty("expiresAtUtc").GetDateTime();

        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            jwtProvider.NotificarLogin(tokenString);

        return new SesionViewModel
        {
            Token = tokenString,
            Username = username,
            Rol = rol,
            ExpiresAtUtc = expiresAt
        };
    }

    private static async Task<ApiErrorResponse> LeerErrorAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return new ApiErrorResponse { Status = (int)response.StatusCode };

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("title", out var title))
                return new ApiErrorResponse
                {
                    Status = (int)response.StatusCode,
                    Title = title.GetString(),
                    Errors = root.TryGetProperty("errors", out var errs)
                        ? errs.EnumerateArray()
                               .Select(e => e.GetString() ?? "")
                               .Where(e => !string.IsNullOrEmpty(e))
                               .ToList()
                        : []
                };

            return new ApiErrorResponse { Status = (int)response.StatusCode };
        }
        catch
        {
            return new ApiErrorResponse { Status = (int)response.StatusCode };
        }
    }
}