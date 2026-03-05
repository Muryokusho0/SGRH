using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        From = from;
        To = (to ?? throw new ArgumentNullException(nameof(to))) is List<EmailRecipient> l1 ? l1 : new List<EmailRecipient>(to);
        if (To.Count == 0) throw new ArgumentException("Debe existir al menos un destinatario (To).", nameof(to));

        Subject = string.IsNullOrWhiteSpace(subject) ? throw new ArgumentException("Subject es requerido.", nameof(subject)) : subject;

        TextBody = textBody;
        HtmlBody = htmlBody;
        Template = template;

        Cc = cc is null ? Array.Empty<EmailRecipient>() : new List<EmailRecipient>(cc);
        Bcc = bcc is null ? Array.Empty<EmailRecipient>() : new List<EmailRecipient>(bcc);
        Attachments = attachments is null ? Array.Empty<EmailAttachment>() : new List<EmailAttachment>(attachments);
    }
}
