using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models.Servicios;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGRH.Web.Services.Servicios;

public sealed class ServicioService : ApiServiceBase, IServicioService
{
    public ServicioService(IHttpClientFactory factory, TokenStorageService tokenStorage, ILogger<ServicioService> logger)
        : base(factory, tokenStorage, logger) { }

    /// <inheritdoc />
    public async Task<List<ServicioViewModel>> ListarAsync(
        DateTime? fechaEntrada = null,
        CancellationToken ct = default)
    {
        // Construir URL con filtro de fecha si se proporciona.
        // La API usa fechaEntrada para determinar la temporada activa
        // y filtrar solo los servicios disponibles para esa temporada.
        var url = fechaEntrada.HasValue
            ? $"api/servicios?fechaEntrada={fechaEntrada.Value:yyyy-MM-dd}"
            : "api/servicios";

        JsonElement json;
        try
        {
            var resp = await CrearCliente().GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    ApiErrorHelper.TraducirError(
                        new Models.ApiErrorResponse { Status = (int)resp.StatusCode }));
            json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex)
        { throw new InvalidOperationException(ApiErrorHelper.TraducirExcepcion(ex)); }

        return json.GetProperty("servicios").EnumerateArray()
            .Select(s => new ServicioViewModel
            {
                ServicioAdicionalId = s.GetProperty("servicioAdicionalId").GetInt32(),
                Nombre = s.GetProperty("nombreServicio").GetString()!,
                Tipo = s.GetProperty("tipoServicio").GetString()!,
                PrecioUnitario = s.TryGetProperty("precioUnitario", out var p)
                                      ? p.GetDecimal() : 0m
            }).ToList();
    }
}