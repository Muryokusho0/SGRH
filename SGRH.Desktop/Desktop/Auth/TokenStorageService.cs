namespace Desktop.Auth;

/// <summary>
/// Almacena el JWT en las Preferences nativas de MAUI.
/// En MAUI Blazor Hybrid la app es un proceso nativo sin "circuito" Blazor Server.
/// Preferences persiste el token entre reinicios de la app en el dispositivo.
/// </summary>
public sealed class TokenStorageService
{
    private const string TokenKey = "sgrh_jwt_token";
    private const string RolKey = "sgrh_rol";
    private const string UserKey = "sgrh_username";

    public void Guardar(string token, string rol, string username)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(RolKey, rol);
        Preferences.Set(UserKey, username);
    }

    public string? GetToken() => Preferences.Get(TokenKey, null);
    public string? GetRol() => Preferences.Get(RolKey, null);
    public string? GetUsername() => Preferences.Get(UserKey, null);

    public bool EstaAutenticado()
        => !string.IsNullOrWhiteSpace(GetToken());

    public bool EsAdmin()
        => GetRol() == "ADMIN";

    public bool EsRecepcionista()
        => GetRol() == "RECEPCIONISTA";

    public void LimpiarSesion()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(RolKey);
        Preferences.Remove(UserKey);
    }
}