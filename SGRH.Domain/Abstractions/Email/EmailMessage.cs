using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Representa un mensaje de correo electrónico completo listo para ser enviado
/// a través del servicio <see cref="IEmailSender"/>.
/// Requiere al menos un destinatario y un cuerpo (texto, HTML o plantilla).
/// </summary>
public sealed class EmailMessage
{
    /// <summary>
    /// Remitente del correo electrónico.
    /// </summary>
    public EmailRecipient From { get; }

    /// <summary>
    /// Lista de destinatarios principales del correo electrónico.
    /// </summary>
    public IReadOnlyList<EmailRecipient> To { get; }

    /// <summary>
    /// Lista de destinatarios en copia (CC). Puede estar vacía.
    /// </summary>
    public IReadOnlyList<EmailRecipient> Cc { get; }

    /// <summary>
    /// Lista de destinatarios en copia oculta (BCC). Puede estar vacía.
    /// </summary>
    public IReadOnlyList<EmailRecipient> Bcc { get; }

    /// <summary>
    /// Asunto del correo electrónico.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Cuerpo del correo en formato texto plano. Puede ser <c>null</c> si se usa HTML o plantilla.
    /// </summary>
    public string? TextBody { get; }

    /// <summary>
    /// Cuerpo del correo en formato HTML. Puede ser <c>null</c> si se usa texto o plantilla.
    /// </summary>
    public string? HtmlBody { get; }

    /// <summary>
    /// Plantilla de correo predefinida con variables de sustitución.
    /// Puede ser <c>null</c> si se usa texto o HTML directo.
    /// </summary>
    public EmailTemplate? Template { get; }

    /// <summary>
    /// Lista de archivos adjuntos incluidos en el correo. Puede estar vacía.
    /// </summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; }

    /// <summary>
    /// Crea un nuevo mensaje de correo electrónico con todos sus componentes.
    /// </summary>
    /// <param name="from">Remitente del mensaje.</param>
    /// <param name="to">Destinatarios principales (debe contener al menos uno).</param>
    /// <param name="subject">Asunto del correo (máx. 255 caracteres).</param>
    /// <param name="textBody">Cuerpo en texto plano (opcional).</param>
    /// <param name="htmlBody">Cuerpo en HTML (opcional).</param>
    /// <param name="template">Plantilla de correo (opcional).</param>
    /// <param name="cc">Destinatarios en copia (opcional).</param>
    /// <param name="bcc">Destinatarios en copia oculta (opcional).</param>
    /// <param name="attachments">Archivos adjuntos (opcional).</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Si no hay destinatarios o si no se proporciona ningún tipo de cuerpo.
    /// </exception>
    public EmailMessage(
        EmailRecipient from,
        IEnumerable<EmailRecipient> to,
        string subject,
        string? textBody = null,
        string? htmlBody = null,
        EmailTemplate? template = null,
        IEnumerable<EmailRecipient>? cc = null,
        IEnumerable<EmailRecipient>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null)
    {
        Guard.AgainstNull(from, nameof(from));
        Guard.AgainstNull(to, nameof(to));
        Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 255);

        To = to.ToList();
        if (To.Count == 0)
            throw new ValidationException("Debe haber al menos un destinatario.");

        // Un email debe tener al menos cuerpo de texto, HTML, o template.
        if (textBody is null && htmlBody is null && template is null)
            throw new ValidationException(
                "El email debe tener TextBody, HtmlBody o Template.");

        From = from;
        Subject = subject;
        TextBody = textBody;
        HtmlBody = htmlBody;
        Template = template;
        Cc = cc?.ToList() ?? [];
        Bcc = bcc?.ToList() ?? [];
        Attachments = attachments?.ToList() ?? [];
    }
}