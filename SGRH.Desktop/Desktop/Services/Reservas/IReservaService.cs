using Desktop.Auth;
using Desktop.Helpers;
using Desktop.Models.Reservas;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Reservas;

public interface IReservaService
{
    Task<List<ReservaResumen>> ListarAsync(CancellationToken ct = default);
    Task<ReservaDetalle?> ObtenerDetalleAsync(int id, CancellationToken ct = default);
    Task<int> CrearAsync(int clienteId, DateTime entrada, DateTime salida, CancellationToken ct = default);
    Task ConfirmarAsync(int id, CancellationToken ct = default);
    Task CancelarAsync(int id, CancellationToken ct = default);
    Task FinalizarAsync(int id, CancellationToken ct = default);
    Task CambiarFechasAsync(int id, DateTime entrada, DateTime salida, CancellationToken ct = default);
    Task AgregarHabitacionAsync(int id, int numeroHabitacion, CancellationToken ct = default);
    Task QuitarHabitacionAsync(int id, int habitacionId, CancellationToken ct = default);
    Task AgregarServicioAsync(int id, int servicioId, int cantidad, CancellationToken ct = default);
    Task QuitarServicioAsync(int id, int servicioId, CancellationToken ct = default);
}

public sealed class ReservaService : ApiServiceBase, IReservaService
{
    public ReservaService(IHttpClientFactory f, TokenStorageService t, ILogger<ReservaService> l)
        : base(f, t, l) { }

    private static StringContent EmptyJson()
        => new("{}", Encoding.UTF8, "application/json");

    public async Task<List<ReservaResumen>> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/reservas",
            () => CrearCliente().GetAsync("api/reservas", ct));
        await EnsureAsync(resp);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("reservas").EnumerateArray()
            .Select(r => new ReservaResumen
            {
                ReservaId = r.GetProperty("reservaId").GetInt32(),
                ClienteNombre = r.TryGetProperty("nombreCliente", out var n) ? n.GetString()! : "",
                Estado = r.GetProperty("estadoReserva").GetString()!,
                FechaReserva = r.GetProperty("fechaReserva").GetDateTime(),
                FechaEntrada = r.GetProperty("fechaEntrada").GetDateTime(),
                FechaSalida = r.GetProperty("fechaSalida").GetDateTime(),
                CostoTotal = r.TryGetProperty("costoTotal", out var c) ? c.GetDecimal() : 0m,
            }).OrderByDescending(x => x.FechaReserva).ToList();
    }

    public async Task<ReservaDetalle?> ObtenerDetalleAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", $"api/reservas/{id}",
            () => CrearCliente().GetAsync($"api/reservas/{id}", ct));
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await EnsureAsync(resp);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var r = json.GetProperty("reserva");
        return new ReservaDetalle
        {
            ReservaId = r.GetProperty("reservaId").GetInt32(),
            ClienteId = r.GetProperty("clienteId").GetInt32(),
            ClienteNombre = r.TryGetProperty("nombreCliente", out var n) ? n.GetString()! : "",
            Estado = r.GetProperty("estadoReserva").GetString()!,
            FechaEntrada = r.GetProperty("fechaEntrada").GetDateTime(),
            FechaSalida = r.GetProperty("fechaSalida").GetDateTime(),
            CostoTotal = r.TryGetProperty("costoTotal", out var c) ? c.GetDecimal() : 0m,
            Habitaciones = r.TryGetProperty("habitaciones", out var habs)
                ? habs.EnumerateArray().Select(h => new HabitacionReserva
                {
                    HabitacionId = h.GetProperty("habitacionId").GetInt32(),
                    NumeroHabitacion = h.GetProperty("numeroHabitacion").GetInt32(),
                    Categoria = h.TryGetProperty("nombreCategoria", out var cat) ? cat.GetString()! : "",
                    TarifaAplicada = h.GetProperty("tarifaAplicada").GetDecimal(),
                }).ToList() : [],
            Servicios = r.TryGetProperty("servicios", out var svcs)
                ? svcs.EnumerateArray().Select(s => new ServicioReserva
                {
                    ServicioAdicionalId = s.GetProperty("servicioAdicionalId").GetInt32(),
                    Nombre = s.GetProperty("nombreServicio").GetString()!,
                    Tipo = s.GetProperty("tipoServicio").GetString()!,
                    Cantidad = s.GetProperty("cantidad").GetInt32(),
                    PrecioUnitario = s.GetProperty("precioUnitario").GetDecimal(),
                    Subtotal = s.GetProperty("subtotal").GetDecimal(),
                }).ToList() : [],
        };
    }

    public async Task<int> CrearAsync(int clienteId, DateTime entrada, DateTime salida, CancellationToken ct = default)
    {
        var payload = new { ClienteId = clienteId, FechaEntrada = entrada, FechaSalida = salida };
        var resp = await LoggedAsync("POST", "api/reservas",
            () => CrearCliente().PostAsJsonAsync("api/reservas", payload, ct));
        await EnsureAsync(resp);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.TryGetProperty("reservaId", out var rid)
            ? rid.GetInt32()
            : json.GetProperty("reserva").GetProperty("reservaId").GetInt32();
    }

    public async Task ConfirmarAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("PATCH", $"api/reservas/{id}/confirmar",
            () => CrearCliente().PatchAsync($"api/reservas/{id}/confirmar", EmptyJson(), ct));
        await EnsureAsync(resp);
    }

    public async Task CancelarAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("PATCH", $"api/reservas/{id}/cancelar",
            () => CrearCliente().PatchAsync($"api/reservas/{id}/cancelar", EmptyJson(), ct));
        await EnsureAsync(resp);
    }

    public async Task FinalizarAsync(int id, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("PATCH", $"api/reservas/{id}/finalizar",
            () => CrearCliente().PatchAsync($"api/reservas/{id}/finalizar", EmptyJson(), ct));
        await EnsureAsync(resp);
    }

    public async Task CambiarFechasAsync(int id, DateTime entrada, DateTime salida, CancellationToken ct = default)
    {
        var payload = new { NuevaEntrada = entrada, NuevaSalida = salida };
        var resp = await LoggedAsync("PATCH", $"api/reservas/{id}/fechas",
            () => CrearCliente().PatchAsJsonAsync($"api/reservas/{id}/fechas", payload, ct));
        await EnsureAsync(resp);
    }

    public async Task AgregarHabitacionAsync(int id, int numeroHabitacion, CancellationToken ct = default)
    {
        var payload = new { NumeroHabitacion = numeroHabitacion };
        var resp = await LoggedAsync("POST", $"api/reservas/{id}/habitaciones",
            () => CrearCliente().PostAsJsonAsync($"api/reservas/{id}/habitaciones", payload, ct));
        await EnsureAsync(resp);
    }

    public async Task QuitarHabitacionAsync(int id, int habitacionId, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("DELETE", $"api/reservas/{id}/habitaciones/{habitacionId}",
            () => CrearCliente().DeleteAsync($"api/reservas/{id}/habitaciones/{habitacionId}", ct));
        await EnsureAsync(resp);
    }

    public async Task AgregarServicioAsync(int id, int servicioId, int cantidad, CancellationToken ct = default)
    {
        var payload = new { ServicioAdicionalId = servicioId, Cantidad = cantidad };
        var resp = await LoggedAsync("POST", $"api/reservas/{id}/servicios",
            () => CrearCliente().PostAsJsonAsync($"api/reservas/{id}/servicios", payload, ct));
        await EnsureAsync(resp);
    }

    public async Task QuitarServicioAsync(int id, int servicioId, CancellationToken ct = default)
    {
        var resp = await LoggedAsync("DELETE", $"api/reservas/{id}/servicios/{servicioId}",
            () => CrearCliente().DeleteAsync($"api/reservas/{id}/servicios/{servicioId}", ct));
        await EnsureAsync(resp);
    }

    private static async Task EnsureAsync(HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                await ApiErrorHelper.LeerErrorAsync(resp));
    }
}