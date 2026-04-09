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
/// </summary>
public abstract class ApiServiceBase
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStorageService _tokenStorage;

    protected ApiServiceBase(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage)
    {
        _factory = factory;
        _tokenStorage = tokenStorage;
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
}