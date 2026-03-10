using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Email;

public sealed class EmailMessage
{
    public EmailRecipient From { get; }
    public IReadOnlyList<EmailRecipient> To { get; }
    public IReadOnlyList<EmailRecipient> Cc { get; }
    public IReadOnlyList<EmailRecipient> Bcc { get; }
    public string Subject { get; }
    public string? TextBody { get; }
    public string? HtmlBody { get; }
    public EmailTemplate? Template { get; }
    public IReadOnlyList<EmailAttachment> Attachments { get; }

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