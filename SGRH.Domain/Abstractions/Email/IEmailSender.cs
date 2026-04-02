using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Define el contrato para el envío de correos electrónicos a través de un proveedor externo.
/// La implementación reside en la capa de infraestructura (por ejemplo, SendGrid o Amazon SES).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Envía un mensaje de correo electrónico de forma asíncrona.
    /// </summary>
    /// <param name="message">Mensaje de correo completo con destinatarios, asunto y cuerpo.</param>
    /// <param name="ct">Token de cancelación para la operación asíncrona.</param>
    /// <returns>
    /// Un <see cref="EmailSendResult"/> indicando si el envío fue exitoso,
    /// el id del mensaje asignado por el proveedor, o el error ocurrido.
    /// </returns>
    Task<EmailSendResult> SendAsync(
        EmailMessage message, CancellationToken ct = default);
}