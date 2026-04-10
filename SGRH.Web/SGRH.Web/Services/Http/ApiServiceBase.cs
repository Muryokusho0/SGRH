using Microsoft.Extensions.Logging;
using SGRH.Web.Auth;
using System.Net.Http.Headers;

namespace SGRH.Web.Services.Http;

/// <summary>
/// Clase base para todos los servicios que consumen SGRH.Api.
///
/// En Blazor Server, IHttpClientFactory resuelve los DelegatingHandlers
/// desde el scope raíz del contenedor DI, no desde el scope del circuito.
/// Por eso el patrón AuthenticatedHttpClientHandler no funciona aquí:
/// TokenStorageService siempre llegaría vacío al handler.
///
/// La solución correcta: inyectar TokenStorageService directamente en cada
/// servicio (ambos son Scoped → mismo scope por circuito) y añadir el
/// header Bearer al momento de crear el HttpClient.
///
/// El ILogger se inyecta aquí para que todos los servicios hereden
/// capacidad de logging sin repetir código. Registra:
///   - Requests salientes (método + URL)
///   - Respuestas con código HTTP
///   - Errores de comunicación (timeout, red caída, etc.)
///   - Errores de negocio devueltos por la API (4xx, 5xx)
/// </summary>
public abstract class ApiServiceBase
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStorageService _tokenStorage;
    private readonly ILogger _logger;

    protected ApiServiceBase(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage,
        ILogger logger)
    {
        _factory = factory;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    /// <summary>
    /// Crea un HttpClient con el header Authorization ya establecido
    /// si hay un token activo en el circuito actual.
    /// </summary>
    protected HttpClient CrearCliente()
    {
        var client = _factory.CreateClient("SGRH.Api");
        var token = _tokenStorage.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Ejecuta una llamada HTTP, registrando el request, la respuesta
    /// y cualquier error de comunicación o de negocio.
    /// </summary>
    /// <param name="method">Método HTTP (GET, POST, PATCH, DELETE).</param>
    /// <param name="url">URL relativa del endpoint.</param>
    /// <param name="llamada">Función que realiza la petición HTTP.</param>
    protected async Task<HttpResponseMessage> LoggedAsync(
        string method, string url,
        Func<Task<HttpResponseMessage>> llamada,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[{Servicio}] {Method} {Url}",
            GetType().Name, method, url);

        HttpResponseMessage resp;
        try
        {
            resp = await llamada();
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning(
                "[{Servicio}] Timeout o cancelación en {Method} {Url}",
                GetType().Name, method, url);
            throw new InvalidOperationException(
                "La solicitud tardó demasiado. Verifica tu conexión e intenta de nuevo.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "[{Servicio}] Error de red en {Method} {Url}",
                GetType().Name, method, url);
            throw new InvalidOperationException(
                "No se pudo conectar con el servidor. Verifica tu conexión.");
        }

        _logger.LogInformation(
            "[{Servicio}] {Method} {Url} → {StatusCode}",
            GetType().Name, method, url, (int)resp.StatusCode);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "[{Servicio}] Respuesta de error {StatusCode} en {Method} {Url}",
                GetType().Name, (int)resp.StatusCode, method, url);
        }

        return resp;
    }
}