using System;

namespace Desktop.Models.Usuarios;

public sealed class UsuarioResumen
{
    public int UsuarioId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CrearUsuarioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Confirmar { get; set; } = string.Empty;
    public string Rol { get; set; } = "RECEPCIONISTA";
}