using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Resultado de un intento de envío de correo electrónico a través del proveedor externo.
/// </summary>
/// <param name="Success">Indica si el correo fue enviado exitosamente.</param>
/// <param name="ProviderMessageId">
/// Identificador del mensaje asignado por el proveedor de correo (por ejemplo, SendGrid o SES).
/// Puede ser <c>null</c> si el envío falló.
/// </param>
/// <param name="Error">
/// Mensaje de error descriptivo si el envío falló. Es <c>null</c> si fue exitoso.
/// </param>
public sealed record EmailSendResult(
    bool Success,
    string? ProviderMessageId = null,
    string? Error = null);
