using SGRH.Web.Models.Auth;

namespace SGRH.Web.Services.Auth;

public interface IAuthService
{
    /// <summary>
    /// Autentica al cliente con Email y Password contra SGRH.Api.
    /// Carga el token automáticamente en el estado de autenticación de Blazor.
    /// </summary>
    Task<SesionViewModel> LoginAsync(LoginViewModel model, CancellationToken ct = default);

    /// <summary>
    /// Registra un nuevo cliente y lo autentica automáticamente.
    /// </summary>
    Task<SesionViewModel> RegisterAsync(RegisterViewModel model, CancellationToken ct = default);

    /// <summary>
    /// Cierra la sesión limpiando el token y notificando a Blazor.
    /// </summary>
    void Logout();
}