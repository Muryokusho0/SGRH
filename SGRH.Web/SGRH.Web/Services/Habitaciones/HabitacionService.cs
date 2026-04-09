using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models.Habitaciones;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGRH.Web.Services.Habitaciones;

public sealed class HabitacionService : ApiServiceBase, IHabitacionService
{
    public HabitacionService(IHttpClientFactory factory, TokenStorageService tokenStorage)
        : base(factory, tokenStorage) { }

    public async Task<List<HabitacionDisponibleViewModel>> ListarDisponiblesAsync(
        DateTime entrada, DateTime salida,
        int? categoriaId = null, CancellationToken ct = default)
    {
        var url = $"api/habitaciones/disponibles" +
                  $"?entrada={entrada:yyyy-MM-dd}" +
                  $"&salida={salida:yyyy-MM-dd}";

        if (categoriaId.HasValue)
            url += $"&categoriaId={categoriaId.Value}";

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

        // El campo Historial se ignora deliberadamente — el cliente no debe verlo
        return json.GetProperty("habitaciones").EnumerateArray()
            .Select(h => new HabitacionDisponibleViewModel
            {
                HabitacionId = h.GetProperty("habitacionId").GetInt32(),
                NumeroHabitacion = h.GetProperty("numeroHabitacion").GetInt32(),
                Piso = h.GetProperty("piso").GetInt32(),
                Categoria = h.GetProperty("nombreCategoria").GetString()!,
                TarifaPorNoche = h.TryGetProperty("tarifaPorNoche", out var t)
                                   ? t.GetDecimal() : 0m
            }).ToList();
    }
}