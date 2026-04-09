using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Auth;

/// <summary>
/// Datos del formulario de login.
/// Las DataAnnotations activan la validación automática de Blazor
/// cuando el formulario usa EditForm + DataAnnotationsValidator.
/// </summary>
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Datos del formulario de registro.
/// Las validaciones de longitud máxima coinciden exactamente con las del
/// RegisterValidator en SGRH.Application para evitar viajes innecesarios a la API.
/// </summary>
public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string NombreCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
    public string ApellidoCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula / identificación es obligatoria.")]
    [MaxLength(20, ErrorMessage = "La cédula no puede superar 20 caracteres.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar tu contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}

/// <summary>
/// Sesión activa del cliente tras login o registro exitoso.
/// Token es internal — solo los servicios HTTP lo usan directamente.
/// </summary>
public sealed class SesionViewModel
{
    internal string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}