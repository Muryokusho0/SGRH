using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Habitaciones;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Habitaciones;

public interface IHabitacionService
{
    Task<List<HabitacionResumen>> ListarAsync(CancellationToken ct = default);
    Task<List<HabitacionDisponible>> ListarDisponiblesAsync(DateTime entrada, DateTime salida, int? categoriaId = null, CancellationToken ct = default);
    Task CambiarEstadoAsync(int id, string estado, string? motivo = null, CancellationToken ct = default);
}

public sealed class HabitacionService : ApiServiceBase, IHabitacionService
{
    public HabitacionService(IHttpClientFactory f, TokenStorageService t, ILogger<HabitacionService> l)
        : base(f, t, l) { }

    public async Task<List<HabitacionResumen>> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/habitaciones",
            () => CrearCliente().GetAsync("api/habitaciones", ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("habitaciones").EnumerateArray()
            .Select(h => new HabitacionResumen
            {
                HabitacionId = h.GetProperty("habitacionId").GetInt32(),
                NumeroHabitacion = h.GetProperty("numeroHabitacion").GetInt32(),
                Piso = h.GetProperty("piso").GetInt32(),
                CategoriaHabitacionId = h.GetProperty("categoriaHabitacionId").GetInt32(),
                Categoria = h.TryGetProperty("nombreCategoria", out var c) ? c.GetString()! : "",
                EstadoActual = h.TryGetProperty("estadoActual", out var e) ? e.GetString()! : "Desconocido",
            }).OrderBy(h => h.NumeroHabitacion).ToList();
    }

    public async Task<List<HabitacionDisponible>> ListarDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaId = null, CancellationToken ct = default)
    {
        var url = $"api/habitaciones/disponibles?fechaEntrada={entrada:yyyy-MM-dd}&fechaSalida={salida:yyyy-MM-dd}";
        if (categoriaId.HasValue) url += $"&categoriaId={categoriaId}";
        var resp = await LoggedAsync("GET", url, () => CrearCliente().GetAsync(url, ct));
        if (!resp.IsSuccessStatusCode) return [];
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("habitaciones").EnumerateArray()
            .Select(h => new HabitacionDisponible
            {
                HabitacionId = h.GetProperty("habitacionId").GetInt32(),
                NumeroHabitacion = h.GetProperty("numeroHabitacion").GetInt32(),
                Piso = h.GetProperty("piso").GetInt32(),
                Categoria = h.TryGetProperty("nombreCategoria", out var c) ? c.GetString()! : "",
                TarifaPorNoche = h.TryGetProperty("tarifaPorNoche", out var t) ? t.GetDecimal() : 0m,
            }).OrderBy(h => h.NumeroHabitacion).ToList();
    }

    public async Task CambiarEstadoAsync(int id, string estado, string? motivo = null, CancellationToken ct = default)
    {
        var payload = new { EstadoHabitacion = estado, MotivoCambio = motivo };
        var resp = await LoggedAsync("PATCH", $"api/habitaciones/{id}/estado",
            () => CrearCliente().PatchAsJsonAsync($"api/habitaciones/{id}/estado", payload, ct));
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorHelper.LeerErrorAsync(resp));
    }
}