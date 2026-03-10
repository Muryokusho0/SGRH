using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Auditoria;

public sealed class AuditoriaEvento : EntityBase
{
    public long AuditoriaEventoId { get; private set; }
    public DateTime FechaUtc { get; private set; }
    public int UsuarioId { get; private set; }           // NOT NULL en BD
    public string Rol { get; private set; } = default!;  // snapshot VARCHAR 20
    public string UsernameSnapshot { get; private set; } = default!; // snapshot NVARCHAR 100

    // ── Qué hizo ───────────────────────────────────
    public string Accion { get; private set; } = default!;  // Ej: CREATE, UPDATE, CANCEL. NVARCHAR 50
    public string Modulo { get; private set; } = default!;  // Ej: Reservas, Tarifas. NVARCHAR 100
    public string Entidad { get; private set; } = default!; // Ej: Reserva. NVARCHAR 100
    public string EntidadId { get; private set; } = default!; // PK como texto. NVARCHAR 64

    // ── Contexto técnico ───────────────────────────
    public Guid RequestId { get; private set; }          // correlación por request
    public string IpOrigen { get; private set; } = default!;  // VARCHAR 45
    public string UserAgent { get; private set; } = default!; // NVARCHAR 255

    // ── Descripción humana ─────────────────────────
    public string Descripcion { get; private set; } = default!; // NVARCHAR 500

    // Colección de cambios campo por campo.
    private readonly List<AuditoriaEventoDetalle> _detalles = [];
    public IReadOnlyCollection<AuditoriaEventoDetalle> Detalles => _detalles;

    private AuditoriaEvento() { }

    public AuditoriaEvento(
        int usuarioId,
        string rol,
        string usernameSnapshot,
        string accion,
        string modulo,
        string entidad,
        string entidadId,
        Guid requestId,
        string ipOrigen,
        string userAgent,
        string descripcion)
    {
        Guard.AgainstOutOfRange(usuarioId, nameof(usuarioId), 0);
        Guard.AgainstNullOrWhiteSpace(rol, nameof(rol), 20);
        Guard.AgainstNullOrWhiteSpace(usernameSnapshot, nameof(usernameSnapshot), 100);
        Guard.AgainstNullOrWhiteSpace(accion, nameof(accion), 50);
        Guard.AgainstNullOrWhiteSpace(modulo, nameof(modulo), 100);
        Guard.AgainstNullOrWhiteSpace(entidad, nameof(entidad), 100);
        Guard.AgainstNullOrWhiteSpace(entidadId, nameof(entidadId), 64);
        Guard.AgainstNullOrWhiteSpace(ipOrigen, nameof(ipOrigen), 45);
        Guard.AgainstNullOrWhiteSpace(userAgent, nameof(userAgent), 255);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 500);

        if (requestId == Guid.Empty)
            throw new Exceptions.ValidationException("RequestId no puede ser un Guid vacío.");

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
        _detalles.Add(new AuditoriaEventoDetalle(
            AuditoriaEventoId, campo, valorAnterior, valorNuevo));
    }

    protected override object GetKey() => AuditoriaEventoId;
}