using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Reportes;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Reportes;

public interface IReporteService
{
    Task<List<OcupacionActiva>> OcupacionActivaAsync(CancellationToken ct = default);
    Task<List<ReservaCosto>> CostosAsync(DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default);
}

public sealed class ReporteService : ApiServiceBase, IReporteService
{
    public ReporteService(IHttpClientFactory f, TokenStorageService t, ILogger<ReporteService> l)
        : base(f, t, l) { }

    public async Task<List<OcupacionActiva>> OcupacionActivaAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/reportes/ocupacion-activa",
            () => CrearCliente().GetAsync("api/reportes/ocupacion-activa", ct));
        if (!resp.IsSuccessStatusCode) return [];
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var arr = json.TryGetProperty("ocupacion", out var o) ? o : json;
        return arr.EnumerateArray().Select(x => new OcupacionActiva
        {
            ReservaId = x.GetProperty("reservaId").GetInt32(),
            HabitacionId = x.GetProperty("habitacionId").GetInt32(),
            HabitacionCodigo = x.GetProperty("habitacionCodigo").GetString()!,
            CategoriaNombre = x.TryGetProperty("categoriaNombre", out var c) ? c.GetString()! : "",
            FechaEntrada = x.GetProperty("fechaEntrada").GetDateTime(),
            FechaSalida = x.GetProperty("fechaSalida").GetDateTime(),
            EstadoReserva = x.GetProperty("estadoReserva").GetString()!,
            ClienteNombre = x.GetProperty("clienteNombre").GetString()!,
        }).ToList();
    }

    public async Task<List<ReservaCosto>> CostosAsync(
        DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
    {
        var url = "api/reportes/costos";
        if (desde.HasValue || hasta.HasValue)
            url += $"?{(desde.HasValue ? $"desde={desde:yyyy-MM-dd}" : "")}{(desde.HasValue && hasta.HasValue ? "&" : "")}{(hasta.HasValue ? $"hasta={hasta:yyyy-MM-dd}" : "")}";
        var resp = await LoggedAsync("GET", url, () => CrearCliente().GetAsync(url, ct));
        if (!resp.IsSuccessStatusCode) return [];
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var arr = json.TryGetProperty("reservas", out var r) ? r : json;
        return arr.EnumerateArray().Select(x => new ReservaCosto
        {
            ReservaId = x.GetProperty("reservaId").GetInt32(),
            ClienteNombre = x.GetProperty("clienteNombre").GetString()!,
            FechaEntrada = x.GetProperty("fechaEntrada").GetDateTime(),
            FechaSalida = x.GetProperty("fechaSalida").GetDateTime(),
            TotalHabitaciones = x.GetProperty("totalHabitaciones").GetDecimal(),
            TotalServicios = x.GetProperty("totalServicios").GetDecimal(),
        }).ToList();
    }
}