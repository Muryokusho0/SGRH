using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models;
using SGRH.Web.Models.Reservas;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SGRH.Web.Services.Reservas;

public sealed class ReservaService : ApiServiceBase, IReservaService
{
    public ReservaService(IHttpClientFactory factory, TokenStorageService tokenStorage, ILogger<ReservaService> logger)
        : base(factory, tokenStorage, logger) { }

    private static StringContent EmptyJsonContent()
        => new("{}", Encoding.UTF8, "application/json");

    public async Task<List<ReservaViewModel>> ListarMisReservasAsync(
        CancellationToken ct = default)
    {
        HttpResponseMessage resp;
        try { resp = await CrearCliente().GetAsync("api/reservas", ct); }
        catch (Exception ex)
        { throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex)); }

        await EnsureSuccessAsync(resp, ct);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("reservas").EnumerateArray()
            .Select(MapearReserva)
            .OrderByDescending(r => r.FechaReserva)
            .ToList();
    }

    public async Task<DetalleReservaViewModel?> ObtenerDetalleAsync(
        int reservaId, CancellationToken ct = default)
    {
        HttpResponseMessage resp;
        try { resp = await CrearCliente().GetAsync($"api/reservas/{reservaId}", ct); }
        catch (Exception ex)
        { throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex)); }

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(resp, ct);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return MapearDetalle(json.GetProperty("reserva"));
    }

    public async Task<int> CrearAsync(
        DateTime fechaEntrada, DateTime fechaSalida, CancellationToken ct = default)
    {
        var payload = new { FechaEntrada = fechaEntrada, FechaSalida = fechaSalida };
        var resp = await EnviarAsync(
            () => CrearCliente().PostAsJsonAsync("api/reservas", payload, ct), ct);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("reservaId").GetInt32();
    }

    public async Task ConfirmarAsync(int reservaId, CancellationToken ct = default)
    {
        var resp = await EnviarAsync(
            () => CrearCliente().PatchAsync(
                $"api/reservas/{reservaId}/confirmar", EmptyJsonContent(), ct), ct);
        resp.Dispose();
    }

    public async Task CancelarAsync(int reservaId, CancellationToken ct = default)
    {
        var resp = await EnviarAsync(
            () => CrearCliente().PatchAsync(
                $"api/reservas/{reservaId}/cancelar", EmptyJsonContent(), ct), ct);
        resp.Dispose();
    }

    public async Task CambiarFechasAsync(
        int reservaId, DateTime nuevaEntrada, DateTime nuevaSalida,
        CancellationToken ct = default)
    {
        var payload = new { NuevaFechaEntrada = nuevaEntrada, NuevaFechaSalida = nuevaSalida };
        var resp = await EnviarAsync(
            () => CrearCliente().PatchAsJsonAsync(
                $"api/reservas/{reservaId}/fechas", payload, ct), ct);
        resp.Dispose();
    }

    public async Task AgregarHabitacionAsync(
        int reservaId, int numeroHabitacion, CancellationToken ct = default)
    {
        var payload = new { NumeroHabitacion = numeroHabitacion };
        var resp = await EnviarAsync(
            () => CrearCliente().PostAsJsonAsync(
                $"api/reservas/{reservaId}/habitaciones", payload, ct), ct);
        resp.Dispose();
    }

    public async Task QuitarHabitacionAsync(
        int reservaId, int habitacionId, CancellationToken ct = default)
    {
        var resp = await EnviarAsync(
            () => CrearCliente().DeleteAsync(
                $"api/reservas/{reservaId}/habitaciones/{habitacionId}", ct), ct);
        resp.Dispose();
    }

    public async Task AgregarServicioAsync(
        int reservaId, int servicioAdicionalId, int cantidad,
        CancellationToken ct = default)
    {
        var payload = new { ServicioAdicionalId = servicioAdicionalId, Cantidad = cantidad };
        var resp = await EnviarAsync(
            () => CrearCliente().PostAsJsonAsync(
                $"api/reservas/{reservaId}/servicios", payload, ct), ct);
        resp.Dispose();
    }

    public async Task QuitarServicioAsync(
        int reservaId, int servicioAdicionalId, CancellationToken ct = default)
    {
        var resp = await EnviarAsync(
            () => CrearCliente().DeleteAsync(
                $"api/reservas/{reservaId}/servicios/{servicioAdicionalId}", ct), ct);
        resp.Dispose();
    }

    private static ReservaViewModel MapearReserva(JsonElement r) => new()
    {
        ReservaId = r.GetProperty("reservaId").GetInt32(),
        Estado = r.GetProperty("estadoReserva").GetString()!,
        FechaReserva = r.GetProperty("fechaReserva").GetDateTime(),
        FechaEntrada = r.GetProperty("fechaEntrada").GetDateTime(),
        FechaSalida = r.GetProperty("fechaSalida").GetDateTime(),
        CostoTotal = r.GetProperty("costoTotal").GetDecimal(),
        TotalHabitaciones = r.GetProperty("habitaciones").GetArrayLength(),
        TotalServicios = r.GetProperty("servicios").GetArrayLength()
    };

    private static DetalleReservaViewModel MapearDetalle(JsonElement r) => new()
    {
        ReservaId = r.GetProperty("reservaId").GetInt32(),
        Estado = r.GetProperty("estadoReserva").GetString()!,
        FechaEntrada = r.GetProperty("fechaEntrada").GetDateTime(),
        FechaSalida = r.GetProperty("fechaSalida").GetDateTime(),
        CostoTotal = r.GetProperty("costoTotal").GetDecimal(),
        Habitaciones = r.GetProperty("habitaciones").EnumerateArray()
            .Select(h => new HabitacionReservaViewModel
            {
                HabitacionId = h.GetProperty("habitacionId").GetInt32(),
                NumeroHabitacion = h.GetProperty("numeroHabitacion").GetInt32(),
                Categoria = h.GetProperty("nombreCategoria").GetString()!,
                TarifaAplicada = h.GetProperty("tarifaAplicada").GetDecimal()
            }).ToList(),
        Servicios = r.GetProperty("servicios").EnumerateArray()
            .Select(s => new ServicioReservaViewModel
            {
                ServicioAdicionalId = s.GetProperty("servicioAdicionalId").GetInt32(),
                Nombre = s.GetProperty("nombreServicio").GetString()!,
                Tipo = s.GetProperty("tipoServicio").GetString()!,
                Cantidad = s.GetProperty("cantidad").GetInt32(),
                PrecioUnitario = s.GetProperty("precioUnitario").GetDecimal(),
                Subtotal = s.GetProperty("subtotal").GetDecimal()
            }).ToList()
    };

    private static async Task<HttpResponseMessage> EnviarAsync(
        Func<Task<HttpResponseMessage>> peticion, CancellationToken ct)
    {
        HttpResponseMessage resp;
        try { resp = await peticion(); }
        catch (Exception ex)
        { throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex)); }

        await EnsureSuccessAsync(resp, ct);
        return resp;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        string mensajeError;
        try
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
            {
                mensajeError = ApiErrorHelper.TraducirError(
                    new ApiErrorResponse { Status = (int)resp.StatusCode });
            }
            else
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("title", out var title) &&
                    !string.IsNullOrEmpty(title.GetString()))
                {
                    var error = new ApiErrorResponse
                    {
                        Status = (int)resp.StatusCode,
                        Title = title.GetString(),
                        Errors = root.TryGetProperty("errors", out var errs)
                            ? errs.EnumerateArray()
                                  .Select(e => e.GetString() ?? "")
                                  .Where(e => !string.IsNullOrEmpty(e))
                                  .ToList()
                            : []
                    };
                    mensajeError = ApiErrorHelper.TraducirError(error);
                }
                else if (root.TryGetProperty("error", out var errorField))
                {
                    mensajeError = ApiErrorHelper.TraducirError(
                        new ApiErrorResponse
                        {
                            Status = (int)resp.StatusCode,
                            Title = errorField.GetString()
                        });
                }
                else
                {
                    mensajeError = ApiErrorHelper.TraducirError(
                        new ApiErrorResponse { Status = (int)resp.StatusCode });
                }
            }
        }
        catch
        {
            mensajeError = ApiErrorHelper.TraducirError(
                new ApiErrorResponse { Status = (int)resp.StatusCode });
        }

        throw new InvalidOperationException(mensajeError);
    }
}