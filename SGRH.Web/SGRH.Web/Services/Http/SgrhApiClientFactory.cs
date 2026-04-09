using System.Net.Http.Headers;
using SGRH.Web.Auth;

namespace SGRH.Web.Services.Http;

/// <summary>
/// Fábrica de HttpClient con scope correcto para Blazor Server.
///
/// PROBLEMA QUE RESUELVE:
/// IHttpClientFactory poolea los DelegatingHandlers y los crea en un scope
/// de DI diferente al circuito Blazor. Esto hace que AuthenticatedHttpClientHandler
/// capture una instancia de TokenStorageService vacía (del root scope),
/// ignorando el token guardado por NotificarLogin (circuit scope).
///
/// SOLUCIÓN:
/// Esta clase es Scoped (una instancia por circuito Blazor).
/// Recibe TokenStorageService del mismo scope → siempre tiene el token correcto.
/// Cada servicio la inyecta y llama a Create() para obtener un HttpClient
/// ya configurado con el Bearer token.
/// </summary>
public sealed class SgrhApiClientFactory
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStorageService _tokenStorage;

    public SgrhApiClientFactory(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage)
    {
        _factory = factory;
        _tokenStorage = tokenStorage;
    }

    /// <summary>
    /// Crea un HttpClient configurado con el token JWT activo del circuito.
    /// Si no hay sesión activa, devuelve el cliente sin cabecera de autorización.
    /// </summary>
    public HttpClient Create()
    {
        var client = _factory.CreateClient("SGRH.Api");
        var token = _tokenStorage.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}