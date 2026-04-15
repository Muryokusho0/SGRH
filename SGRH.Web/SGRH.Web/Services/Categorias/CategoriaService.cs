using Microsoft.Extensions.Caching.Memory;
using SGRH.Web.Auth;
using SGRH.Web.Helpers;
using SGRH.Web.Models.Categorias;
using SGRH.Web.Services.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGRH.Web.Services.Categorias;

/// <summary>
/// Servicio de categorías con caché en memoria.
///
/// Las categorías de habitación son datos de catálogo que el administrador
/// modifica raramente. Cachearlas 30 minutos elimina consultas repetitivas
/// a la API en cada vez que el usuario abre el wizard de nueva reserva
/// o la página de habitaciones disponibles.
/// </summary>
public sealed class CategoriaService : ApiServiceBase, ICategoriaService
{
    private readonly IMemoryCache _cache;

    // Clave única en el caché — una por instancia del circuito no es necesario
    // porque las categorías son las mismas para todos los usuarios.
    private const string CacheKey = "categorias_all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public CategoriaService(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage,
        IMemoryCache cache,
        ILogger<CategoriaService> logger)
        : base(factory, tokenStorage, logger)
    {
        _cache = cache;
    }

    public async Task<List<CategoriaViewModel>> ListarAsync(
        CancellationToken ct = default)
    {
        // Intenta obtener del caché primero
        if (_cache.TryGetValue(CacheKey, out List<CategoriaViewModel>? cached)
            && cached is not null)
            return cached;

        // Si no está en caché, consulta la API
        JsonElement json;
        try
        {
            var resp = await CrearCliente().GetAsync("api/categorias", ct);
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

        var resultado = json.GetProperty("categorias").EnumerateArray()
            .Select(c => new CategoriaViewModel
            {
                CategoriaHabitacionId = c.GetProperty("categoriaHabitacionId").GetInt32(),
                Nombre = c.GetProperty("nombreCategoria").GetString()!,
                Capacidad = c.GetProperty("capacidad").GetInt32(),
                Descripcion = c.GetProperty("descripcion").GetString()!,
                PrecioBase = c.GetProperty("precioBase").GetDecimal()
            }).ToList();

        // Guardar en caché con expiración absoluta
        _cache.Set(CacheKey, resultado, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            // Si la entrada no se usa durante 10 minutos, se descarta antes
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });

        return resultado;
    }
}