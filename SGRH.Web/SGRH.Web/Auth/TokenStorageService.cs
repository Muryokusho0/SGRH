namespace SGRH.Web.Auth;

/// <summary>
/// Almacena el token JWT en memoria durante la vida del circuito Blazor.
/// Scoped: una instancia por pestaña del navegador (por circuito).
/// El cliente nunca interactúa con este servicio directamente;
/// es consumido por <see cref="JwtAuthenticationStateProvider"/>
/// y por <see cref="Services.Http.AuthenticatedHttpClientHandler"/>.
/// </summary>
public sealed class TokenStorageService
{
    private string? _token;

    /// <summary>
    /// Devuelve el token JWT activo, o <c>null</c> si no hay sesión iniciada.
    /// </summary>
    public string? GetToken() => _token;

    /// <summary>
    /// Guarda el token JWT tras un login exitoso.
    /// </summary>
    /// <param name="token">Token JWT devuelto por la API.</param>
    public void SetToken(string token) => _token = token;

    /// <summary>
    /// Elimina el token al cerrar sesión.
    /// </summary>
    public void ClearToken() => _token = null;

    /// <summary>
    /// Indica si hay una sesión activa con token válido.
    /// </summary>
    public bool HasToken() => !string.IsNullOrWhiteSpace(_token);
}