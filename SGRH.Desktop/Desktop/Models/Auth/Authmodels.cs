namespace Desktop.Models.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class SesionActual
{
    public string Token { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}