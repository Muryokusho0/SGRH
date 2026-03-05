using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Auditoria;

public sealed class AuditoriaEvento : EntityBase
{
    public long AuditoriaEventoId { get; private set; }

    // En BD es FechaUtc con default SYSUTCDATETIME()
    public DateTime FechaUtc { get; private set; } = DateTime.UtcNow;

    // Quién ejecutó
    public int? UsuarioId { get; private set; }
    public string? Rol { get; private set; }                 // snapshot (varchar 20)
    public string? UsernameSnapshot { get; private set; }    // snapshot (nvarchar 100)

    // Qué hizo
    public string Accion { get; private set; } = default!;   // nvarchar 50
    public string Modulo { get; private set; } = default!;   // nvarchar 100
    public string? Entidad { get; private set; }             // nvarchar 100
    public string? EntidadId { get; private set; }           // nvarchar 64

    // Contexto técnico
    public Guid? RequestId { get; private set; }
    public string? IpOrigen { get; private set; }            // varchar 45
    public string? UserAgent { get; private set; }           // nvarchar 255

    // Descripción humana
    public string? Descripcion { get; private set; }         // nvarchar 500

    private readonly List<AuditoriaEventoDetalle> _detalles = [];
    public IReadOnlyCollection<AuditoriaEventoDetalle> Detalles => _detalles;

    private AuditoriaEvento() { }

    public AuditoriaEvento(
        int? usuarioId,
        string? rol,
        string? usernameSnapshot,
        string accion,
        string modulo,
        string? entidad = null,
        string? entidadId = null,
        Guid? requestId = null,
        string? ipOrigen = null,
        string? userAgent = null,
        string? descripcion = null)
    {
        Guard.AgainstNullOrWhiteSpace(accion, nameof(accion), 50);
        Guard.AgainstNullOrWhiteSpace(modulo, nameof(modulo), 100);

        if (rol is not null && rol.Length > 20)
            throw new ValidationException("Rol excede 20 caracteres.");

        if (usernameSnapshot is not null && usernameSnapshot.Length > 100)
            throw new ValidationException("UsernameSnapshot excede 100 caracteres.");

        if (entidad is not null && entidad.Length > 100)
            throw new ValidationException("Entidad excede 100 caracteres.");

        if (entidadId is not null && entidadId.Length > 64)
            throw new ValidationException("EntidadId excede 64 caracteres.");

        if (ipOrigen is not null && ipOrigen.Length > 45)
            throw new ValidationException("IpOrigen excede 45 caracteres.");

        if (userAgent is not null && userAgent.Length > 255)
            throw new ValidationException("UserAgent excede 255 caracteres.");

        if (descripcion is not null && descripcion.Length > 500)
            throw new ValidationException("Descripcion excede 500 caracteres.");

        UsuarioId = usuarioId;
        Rol = rol;
        UsernameSnapshot = usernameSnapshot;

        Accion = accion;
        Modulo = modulo;
        Entidad = entidad;
        EntidadId = entidadId;

        RequestId = requestId;
        IpOrigen = ipOrigen;
        UserAgent = userAgent;

        Descripcion = descripcion;

        FechaUtc = DateTime.UtcNow;
    }

    public void AgregarDetalle(string campo, string? valorAnterior, string? valorNuevo)
    {
        Guard.AgainstNullOrWhiteSpace(campo, nameof(campo), 128);

        _detalles.Add(new AuditoriaEventoDetalle(
            auditoriaEventoId: AuditoriaEventoId,
            campo: campo,
            valorAnterior: valorAnterior,
            valorNuevo: valorNuevo));
    }

    protected override object GetKey() => AuditoriaEventoId;
}