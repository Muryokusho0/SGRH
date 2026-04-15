using Desktop.Auth;
using Desktop.Services.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.Services.Categorias;

public sealed class CategoriasService : ApiServiceBase
{
    public CategoriasService(IHttpClientFactory f, TokenStorageService t, ILogger<CategoriasService> l)
        : base(f, t, l) { }

    public async Task<JsonElement> ListarAsync(CancellationToken ct = default)
    {
        var resp = await LoggedAsync("GET", "api/categorias", () => CrearCliente().GetAsync("api/categorias", ct));
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }
}