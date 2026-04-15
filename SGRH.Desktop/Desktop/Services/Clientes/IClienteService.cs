using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Clientes;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Clientes;

public interface IClienteService
{
    Task<List<ClienteResumen>> ListarAsync(string? nombre = null, CancellationToken ct = default);
    Task<int> CrearAsync(CrearClienteRequest request, CancellationToken ct = default);
}

public sealed class ClienteService : ApiServiceBase, IClienteService
{
    public ClienteService(IHttpClientFactory f, TokenStorageService t, ILogger<ClienteService> l)
        : base(f, t, l) { }

    public async Task<List<ClienteResumen>> ListarAsync(string? nombre = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(nombre) ? "api/clientes" : $"api/clientes?nombre={nombre}";
        var resp = await LoggedAsync("GET", url, () => CrearCliente().GetAsync(url, ct));
        if (!resp.IsSuccessStatusCode) return [];
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("clientes").EnumerateArray()
            .Select(c => new ClienteResumen
            {
                ClienteId = c.GetProperty("clienteId").GetInt32(),
                NationalId = c.GetProperty("nationalId").GetString()!,
                NombreCompleto = $"{c.GetProperty("nombreCliente").GetString()} {c.GetProperty("apellidoCliente").GetString()}",
                Email = c.GetProperty("email").GetString()!,
                Telefono = c.GetProperty("telefono").GetString()!,
            }).ToList();
    }

    public async Task<int> CrearAsync(CrearClienteRequest request, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("POST", "api/clientes",
            () => CrearCliente().PostAsJsonAsync("api/clientes", request, ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.TryGetProperty("clienteId", out var cid)
            ? cid.GetInt32()
            : json.GetProperty("cliente").GetProperty("clienteId").GetInt32();
    }
}