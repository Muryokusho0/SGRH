using Desktop.Auth;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Servicios;

public sealed class ServiciosService : ApiServiceBase
{
    public ServiciosService(IHttpClientFactory f, TokenStorageService t, ILogger<ServiciosService> l)
        : base(f, t, l) { }

    public async Task<JsonElement> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/servicios", () => CrearCliente().GetAsync("api/servicios", ct));
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }
}