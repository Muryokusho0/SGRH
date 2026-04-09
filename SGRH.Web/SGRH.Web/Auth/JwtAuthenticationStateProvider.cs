using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SGRH.Web.Auth;

/// <summary>
/// Proveedor de estado de autenticación para Blazor Server.
/// Lee el token JWT del <see cref="TokenStorageService"/>,
/// extrae los claims y construye el <see cref="ClaimsPrincipal"/>.
///
/// Flujo de login automático:
/// 1. <c>AuthService.LoginAsync</c> recibe el token de la API.
/// 2. Llama a <see cref="NotificarLogin"/> con el token.
/// 3. Este proveedor guarda el token y notifica a Blazor.
/// 4. Blazor propaga el nuevo <see cref="AuthenticationState"/>
///    en cascada a todos los componentes de la UI de forma inmediata.
///    El cliente no necesita hacer nada manualmente.
/// </summary>
public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;

    /// <summary>Estado anónimo reutilizable (sin allocations innecesarias).</summary>
    private static readonly AuthenticationState _estadoAnonimo =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(TokenStorageService tokenStorage)
        => _tokenStorage = tokenStorage;

    /// <inheritdoc />
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokenStorage.GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(_estadoAnonimo);

        try
        {
            var claims = ExtraerClaims(token);
            var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
            var usuario = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(usuario));
        }
        catch
        {
            // Token malformado o expirado: tratar la sesión como anónima
            // y limpiar el almacenamiento para evitar loops.
            _tokenStorage.ClearToken();
            return Task.FromResult(_estadoAnonimo);
        }
    }

    /// <summary>
    /// Llama a este método justo después de un login exitoso.
    /// Guarda el token y notifica a Blazor para que actualice
    /// el estado de autenticación en toda la UI de forma automática.
    /// </summary>
    /// <param name="token">Token JWT devuelto por la API.</param>
    public void NotificarLogin(string token)
    {
        _tokenStorage.SetToken(token);

        var claims = ExtraerClaims(token);
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        var usuario = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(usuario)));
    }

    /// <summary>
    /// Llama a este método al cerrar sesión.
    /// Limpia el token y notifica a Blazor para que revierta
    /// el estado a anónimo en toda la UI.
    /// </summary>
    public void NotificarLogout()
    {
        _tokenStorage.ClearToken();
        NotifyAuthenticationStateChanged(Task.FromResult(_estadoAnonimo));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Parsea el token JWT y devuelve sus claims sin validar la firma
    /// (la validación de firma es responsabilidad de SGRH.Api).
    /// </summary>
    private static IEnumerable<Claim> ExtraerClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }
}