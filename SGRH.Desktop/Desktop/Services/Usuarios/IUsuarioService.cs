using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Usuarios;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Usuarios;

public interface IUsuarioService
{
    Task<List<UsuarioResumen>> ListarAsync(CancellationToken ct = default);
    Task CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task DesactivarAsync(int id, CancellationToken ct = default);
    Task ActivarAsync(int id, CancellationToken ct = default);
}

public sealed class UsuarioService : ApiServiceBase, IUsuarioService
{
    public UsuarioService(IHttpClientFactory f, TokenStorageService t, ILogger<UsuarioService> l)
        : base(f, t, l) { }

    public async Task<List<UsuarioResumen>> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/usuarios",
            () => CrearCliente().GetAsync("api/usuarios", ct));
        if (!resp.IsSuccessStatusCode) return [];
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("usuarios").EnumerateArray()
            .Select(u => new UsuarioResumen
            {
                UsuarioId = u.GetProperty("usuarioId").GetInt32(),
                Username = u.GetProperty("username").GetString()!,
                Rol = u.GetProperty("rol").GetString()!,
                Activo = u.GetProperty("activo").GetBoolean(),
                CreatedAt = u.GetProperty("createdAt").GetDateTime(),
            }).ToList();
    }

    public async Task CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            Username = request.Username,
            Password = request.Password,
            ConfirmarPassword = request.Confirmar,
            Rol = request.Rol
        };
        var resp = await LoggedAsync("POST", "api/auth/register",
            () => CrearCliente().PostAsJsonAsync("api/auth/register", payload, ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
    }

    public async Task DesactivarAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("PATCH", $"api/usuarios/{id}/desactivar",
            () => CrearCliente().PatchAsync($"api/usuarios/{id}/desactivar",
                new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json"), ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
    }

    public async Task ActivarAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("PATCH", $"api/usuarios/{id}/activar",
            () => CrearCliente().PatchAsync($"api/usuarios/{id}/activar",
                new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json"), ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
    }
}