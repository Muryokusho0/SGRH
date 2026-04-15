using Desktop.Auth;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Desktop.Services.Http;

public abstract class ApiServiceBase
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStorageService _token;
    private readonly ILogger _logger;

    protected ApiServiceBase(
        IHttpClientFactory factory,
        TokenStorageService token,
        ILogger logger)
    {
        _factory = factory;
        _token = token;
        _logger = logger;
    }

    protected HttpClient CrearCliente()
    {
        var client = _factory.CreateClient("SGRH.Api");
        var jwt = _token.GetToken();
        if (!string.IsNullOrWhiteSpace(jwt))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }

    protected async Task<HttpResponseMessage> LoggedAsync(
        string method, string url,
        Func<Task<HttpResponseMessage>> llamada)
    {
        _logger.LogInformation("[{S}] {M} {U}", GetType().Name, method, url);
        HttpResponseMessage resp;
        try { resp = await llamada(); }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("[{S}] Timeout en {M} {U}", GetType().Name, method, url);
            throw new InvalidOperationException("La solicitud tardó demasiado. Verifica tu conexión.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[{S}] Error de red en {M} {U}", GetType().Name, method, url);
            throw new InvalidOperationException("No se pudo conectar con el servidor.");
        }
        _logger.LogInformation("[{S}] {M} {U} → {C}", GetType().Name, method, url, (int)resp.StatusCode);
        return resp;
    }
}