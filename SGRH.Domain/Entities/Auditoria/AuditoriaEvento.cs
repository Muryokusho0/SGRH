using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Entities.Auditoria;

/// <summary>
/// Registra un evento de auditoría generado por una acción de usuario en el sistema.
/// Cada evento puede tener múltiples detalles que describen los cambios realizados campo a campo.
/// </summary>
public sealed class AuditoriaEvento : EntityBase
{
    /// <summary>
    /// Identificador único del evento de auditoría.
    /// </summary>
    public long AuditoriaEventoId { get; private set; }

    /// <summary>
    /// Fecha y hora (en hora local UTC-4) en que ocurrió el evento.
    /// </summary>
    public DateTime FechaUtc { get; private set; }

    /// <summary>
    /// Identificador del usuario que realizó la acción auditada.
    /// </summary>
    public int UsuarioId { get; private set; }

    /// <summary>
    /// Rol del usuario en el momento en que ocurrió el evento (snapshot).
    /// </summary>
    public string Rol { get; private set; } = default!;

    /// <summary>
    /// Nombre de usuario (username) en el momento del evento (snapshot).
    /// </summary>
    public string UsernameSnapshot { get; private set; } = default!;

    /// <summary>
    /// Nombre de la acción ejecutada (por ejemplo: "Crear", "Actualizar", "Eliminar").
    /// </summary>
    public string Accion { get; private set; } = default!;

    /// <summary>
    /// Módulo funcional del sistema donde ocurrió la acción (por ejemplo: "Reservas", "Habitaciones").
    /// </summary>
    public string Modulo { get; private set; } = default!;

    /// <summary>
    /// Nombre del tipo de entidad afectada (por ejemplo: "Reserva", "Cliente").
    /// </summary>
    public string Entidad { get; private set; } = default!;

    /// <summary>
    /// Identificador de la instancia concreta de la entidad afectada.
    /// </summary>
    public string EntidadId { get; private set; } = default!;

    /// <summary>
    /// Identificador único del request HTTP que originó el evento. Permite correlación con logs.
    /// </summary>
    public Guid RequestId { get; private set; }

    /// <summary>
    /// Dirección IP desde la que se realizó la solicitud.
    /// </summary>
    public string IpOrigen { get; private set; } = default!;

    /// <summary>
    /// User-Agent del cliente HTTP que realizó la solicitud.
    /// </summary>
    public string UserAgent { get; private set; } = default!;

    /// <summary>
    /// Descripción legible del evento que resume lo ocurrido.
    /// </summary>
    public string Descripcion { get; private set; } = default!;

    private readonly List<AuditoriaEventoDetalle> _detalles = [];

    /// <summary>
    /// Colección de detalles campo a campo de los cambios realizados en este evento.
    /// </summary>
    public IReadOnlyCollection<AuditoriaEventoDetalle> Detalles => _detalles;

    private AuditoriaEvento() { }

    /// <summary>
    /// Inicializa un nuevo evento de auditoría con toda la información contextual requerida.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario que realizó la acción.</param>
    /// <param name="rol">Rol del usuario al momento del evento.</param>
    /// <param name="usernameSnapshot">Nombre de usuario al momento del evento.</param>
    /// <param name="accion">Nombre de la acción ejecutada.</param>
    /// <param name="modulo">Módulo del sistema donde ocurrió la acción.</param>
    /// <param name="entidad">Tipo de entidad afectada.</param>
    /// <param name="entidadId">Identificador de la instancia de entidad afectada.</param>
    /// <param name="requestId">Identificador único del request HTTP.</param>
    /// <param name="ipOrigen">Dirección IP de origen de la solicitud.</param>
    /// <param name="userAgent">User-Agent del cliente HTTP.</param>
    /// <param name="descripcion">Descripción legible del evento.</param>
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

    /// <summary>
    /// Agrega un detalle de cambio al evento de auditoría, registrando el valor anterior
    /// y el nuevo de un campo específico de la entidad modificada.
    /// </summary>
    /// <param name="campo">Nombre del campo que fue modificado.</param>
    /// <param name="valorAnterior">Valor previo al cambio. Si era nulo, se almacena cadena vacía.</param>
    /// <param name="valorNuevo">Valor posterior al cambio. Si es nulo, se almacena cadena vacía.</param>
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