using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Contiene información contextual adicional sobre el entorno en que ocurrió
/// un error o advertencia, utilizada para enriquecer las notificaciones enviadas al administrador.
/// </summary>
public sealed class AdminNotificationContext
{
    /// <summary>
    /// Identificador único del request HTTP en que ocurrió el evento.
    /// Permite correlacionar la notificación con los logs del sistema.
    /// </summary>
    public Guid? RequestId { get; init; }

    /// <summary>
    /// Endpoint o acción que se estaba ejecutando al momento del error
    /// (por ejemplo: "POST /api/reservas/confirmar").
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Nombre del usuario autenticado al momento del evento, si había sesión activa.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Dirección IP de origen del request que originó el evento.
    /// </summary>
    public string? IpOrigen { get; init; }

    /// <summary>
    /// Datos adicionales de contexto en formato clave-valor.
    /// Puede incluir parámetros relevantes, ids de entidades, etc.
    /// Ejemplo: <c>new Dictionary { ["ReservaId"] = "42", ["Accion"] = "Confirmar" }</c>
    /// </summary>
    public IDictionary<string, string>? Extra { get; init; }
}
