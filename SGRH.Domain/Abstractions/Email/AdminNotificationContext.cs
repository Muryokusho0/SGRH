using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

public sealed class AdminNotificationContext
{
    // ID del request HTTP si aplica. Para correlacionar con los logs.
    public Guid? RequestId { get; init; }

    // Endpoint o acción que estaba ejecutándose cuando ocurrió el error.
    public string? Endpoint { get; init; }

    // Usuario autenticado en el momento del error (si había sesión activa).
    public string? Username { get; init; }

    // IP de origen de la request.
    public string? IpOrigen { get; init; }

    // Datos adicionales libres: parámetros relevantes, IDs de entidades, etc.
    // Ejemplo: new Dictionary { ["ReservaId"] = "42", ["Accion"] = "Confirmar" }
    public IDictionary<string, string>? Extra { get; init; }
}
