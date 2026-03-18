using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Auditoria;

public sealed class AuditoriaEvento : EntityBase
{
    public long AuditoriaEventoId { get; private set; }
    public DateTime FechaUtc { get; private set; }
    public int UsuarioId { get; private set; }
    public string Rol { get; private set; } = default!;
    public string UsernameSnapshot { get; private set; } = default!;

    public string Accion { get; private set; } = default!;
    public string Modulo { get; private set; } = default!;
    public string Entidad { get; private set; } = default!;
    public string EntidadId { get; private set; } = default!;

    public Guid RequestId { get; private set; }
    public string IpOrigen { get; private set; } = default!;
    public string UserAgent { get; private set; } = default!;
    public string Descripcion { get; private set; } = default!;

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
            throw new ValidationException("RequestId no puede ser un Guid vacío.");

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

        // ← Hora local UTC-4, no UTC
        FechaUtc = HoraLocal.Ahora;
    }

    public void AgregarDetalle(string campo, string? valorAnterior, string? valorNuevo)
    {
        // auditoriaEventoId puede ser 0 si el evento aún no fue guardado.
        // EF Core propaga el FK automáticamente al hacer SaveChanges.
        // ValorAnterior y ValorNuevo son NOT NULL en BD — convertir null a string.Empty.
        // Semánticamente: string.Empty = "existía pero estaba vacío/sin valor",
        // lo cual es correcto para campos opcionales como MotivoCambio.
        _detalles.Add(new AuditoriaEventoDetalle(
            AuditoriaEventoId, campo, valorAnterior ?? string.Empty, valorNuevo ?? string.Empty));
    }

    protected override object GetKey() => AuditoriaEventoId;
}