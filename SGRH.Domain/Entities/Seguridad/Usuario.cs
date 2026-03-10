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

public sealed class Usuario : EntityBase
{
    public int UsuarioId { get; private set; }
    public int? ClienteId { get; private set; }
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }

    // Fecha de creación del usuario. Se asigna una sola vez en el constructor.
    public DateTime CreatedAtUtc { get; private set; }

    private Usuario() { }

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

    public void CambiarPassword(string nuevoHash)
    {
        Guard.AgainstNullOrWhiteSpace(nuevoHash, nameof(nuevoHash), 255);
        PasswordHash = nuevoHash;
    }

    public void Desactivar()
    {
        if (!Activo)
            throw new BusinessRuleViolationException("El usuario ya está inactivo.");

        Activo = false;
    }

    public void Activar()
    {
        if (Activo)
            throw new BusinessRuleViolationException("El usuario ya está activo.");

        Activo = true;
    }

    protected override object GetKey() => UsuarioId;
}
