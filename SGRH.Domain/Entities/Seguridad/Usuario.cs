using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Enums;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Seguridad;

/// <summary>
/// Representa una cuenta de usuario del sistema con credenciales de acceso y rol asignado.
/// Los usuarios con rol <see cref="RolUsuario.CLIENTE"/> deben tener un <c>ClienteId</c> asociado.
/// Los usuarios con rol <see cref="RolUsuario.ADMIN"/> o <see cref="RolUsuario.RECEPCIONISTA"/>
/// no deben tener <c>ClienteId</c>.
/// </summary>
public sealed class Usuario : EntityBase
{
    /// <summary>
    /// Identificador único del usuario en el sistema.
    /// </summary>
    public int UsuarioId { get; private set; }

    /// <summary>
    /// Identificador del cliente asociado al usuario. Solo aplica cuando el rol es
    /// <see cref="RolUsuario.CLIENTE"/>; es <c>null</c> para ADMIN y RECEPCIONISTA.
    /// </summary>
    public int? ClienteId { get; private set; }

    /// <summary>
    /// Nombre de usuario único utilizado para iniciar sesión.
    /// </summary>
    public string Username { get; private set; } = default!;

    /// <summary>
    /// Hash de la contraseña del usuario, generado con el servicio <c>IPasswordHasher</c>.
    /// No almacena la contraseña en texto plano.
    /// </summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>
    /// Rol de acceso del usuario, que determina sus permisos en el sistema.
    /// </summary>
    public RolUsuario Rol { get; private set; }

    /// <summary>
    /// Indica si la cuenta de usuario está activa. Los usuarios inactivos no pueden iniciar sesión.
    /// </summary>
    public bool Activo { get; private set; }

    /// <summary>
    /// Fecha y hora en UTC en que fue creada la cuenta de usuario. Inmutable tras la creación.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    private Usuario() { }

    /// <summary>
    /// Crea un nuevo usuario con sus credenciales y rol asignado.
    /// </summary>
    /// <param name="clienteId">
    /// Id del cliente asociado. Requerido si <paramref name="rol"/> es
    /// <see cref="RolUsuario.CLIENTE"/>; debe ser <c>null</c> para otros roles.
    /// </param>
    /// <param name="username">Nombre de usuario único (máx. 100 caracteres).</param>
    /// <param name="passwordHash">Hash de la contraseña generado externamente (máx. 255 caracteres).</param>
    /// <param name="rol">Rol de acceso del nuevo usuario.</param>
    /// <exception cref="Exceptions.BusinessRuleViolationException">
    /// Si el rol CLIENTE no tiene ClienteId, o si un rol de sistema tiene ClienteId asociado.
    /// </exception>
    public Usuario(
        int? clienteId,
        string username,
        string passwordHash,
        RolUsuario rol)
    {
        Guard.AgainstNullOrWhiteSpace(username, nameof(username), 100);
        Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash), 255);

        // CLIENTE debe tener ClienteId asociado.
        if (rol == RolUsuario.CLIENTE && clienteId is null)
            throw new BusinessRuleViolationException(
                "Un usuario con rol CLIENTE debe tener un ClienteId asociado.");

        // ADMIN y RECEPCIONISTA NO deben tener ClienteId.
        if (rol != RolUsuario.CLIENTE && clienteId is not null)
            throw new BusinessRuleViolationException(
                "Un usuario con rol ADMIN o RECEPCIONISTA no debe tener ClienteId.");

        ClienteId = clienteId;
        Username = username;
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el hash de contraseña del usuario con un nuevo valor generado externamente.
    /// </summary>
    /// <param name="nuevoHash">Nuevo hash de contraseña (máx. 255 caracteres).</param>
    public void CambiarPassword(string nuevoHash)
    {
        Guard.AgainstNullOrWhiteSpace(nuevoHash, nameof(nuevoHash), 255);
        PasswordHash = nuevoHash;
    }

    /// <summary>
    /// Desactiva la cuenta del usuario, impidiendo el inicio de sesión.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si el usuario ya está inactivo.</exception>
    public void Desactivar()
    {
        if (!Activo)
            throw new BusinessRuleViolationException("El usuario ya está inactivo.");

        Activo = false;
    }

    /// <summary>
    /// Reactiva la cuenta del usuario, permitiendo nuevamente el inicio de sesión.
    /// </summary>
    /// <exception cref="Exceptions.BusinessRuleViolationException">Si el usuario ya está activo.</exception>
    public void Activar()
    {
        if (Activo)
            throw new BusinessRuleViolationException("El usuario ya está activo.");

        Activo = true;
    }

    protected override object GetKey() => UsuarioId;
}
