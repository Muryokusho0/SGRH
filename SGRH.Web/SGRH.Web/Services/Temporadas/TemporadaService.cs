using Microsoft.Extensions.Caching.Memory;
using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models.Temporadas;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGRH.Web.Services.Temporadas;

/// <summary>
/// Servicio de temporadas con caché en memoria.
///
/// Las temporadas cambian con muy poca frecuencia (las configura el administrador).
/// Cachearlas 60 minutos evita N consultas cada vez que el cliente navega
/// a la página de temporadas o que ServicioService filtra por fechaEntrada.
/// </summary>
public sealed class TemporadaService : ApiServiceBase, ITemporadaService
{
    private readonly IMemoryCache _cache;

    private const string CacheKey = "temporadas_all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    public TemporadaService(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage,
        IMemoryCache cache,
        ILogger<TemporadaService> logger)
        : base(factory, tokenStorage, logger)
    {
        _cache = cache;
    }

    public async Task<List<TemporadaViewModel>> ListarAsync(
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out List<TemporadaViewModel>? cached)
            && cached is not null)
            return cached;

        JsonElement json;
        try
        {
            var resp = await CrearCliente().GetAsync("api/temporadas", ct);
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    ApiErrorHelper.TraducirError(
                        new Models.ApiErrorResponse { Status = (int)resp.StatusCode }));
            json = await resp.Content
                .ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                ApiErrorHelper.TraducirExcepcion(ex));
        }

        var resultado = json.GetProperty("temporadas").EnumerateArray()
            .Select(t => new TemporadaViewModel
            {
                Nombre = t.GetProperty("nombreTemporada").GetString()!,
                FechaInicio = t.TryGetProperty("fechaInicio", out var fi)
                              && fi.ValueKind != JsonValueKind.Null
                              ? fi.GetDateTime() : null,
                FechaFin = t.TryGetProperty("fechaFin", out var ff)
                              && ff.ValueKind != JsonValueKind.Null
                              ? ff.GetDateTime() : null
            })
            .OrderByDescending(t => t.EsActual)
            .ThenByDescending(t => t.FechaInicio)
            .ToList();

        _cache.Set(CacheKey, resultado, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromMinutes(15)
        });

        return resultado;
    }
}