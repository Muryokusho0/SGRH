using Desktop.Auth;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Temporadas;

public sealed class TemporadasService : ApiServiceBase
{
    public TemporadasService(IHttpClientFactory f, TokenStorageService t, ILogger<TemporadasService> l)
        : base(f, t, l) { }

    public async Task<JsonElement> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/temporadas", () => CrearCliente().GetAsync("api/temporadas", ct));
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }
}