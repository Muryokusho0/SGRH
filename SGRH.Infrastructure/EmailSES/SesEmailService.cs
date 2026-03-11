using SesV2 = Amazon.SimpleEmailV2;
using SesV2Model = Amazon.SimpleEmailV2.Model;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using SGRH.Domain.Abstractions.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.EmailSES;

public sealed class SesEmailSender : IEmailSender
{
    private readonly SesV2.IAmazonSimpleEmailServiceV2 _ses;
    private readonly SesOptions _options;

    public SesEmailSender(
        SesV2.IAmazonSimpleEmailServiceV2 ses,
        IOptions<SesOptions> options)
    {
        _ses = ses;
        _options = options.Value;
    }

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            if (message.Template is not null)
                return await SendWithTemplateAsync(message, ct);

            if (message.Attachments.Count > 0)
                return await SendRawAsync(message, ct);

            return await SendSimpleAsync(message, ct);
        }
        catch (SesV2Model.SendingPausedException ex)
        {
            return new EmailSendResult(Success: false,
                Error: $"Envío SES pausado: {ex.Message}");
        }
        catch (SesV2Model.MessageRejectedException ex)
        {
            return new EmailSendResult(Success: false,
                Error: $"Mensaje rechazado por SES: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new EmailSendResult(Success: false,
                Error: $"Error inesperado al enviar email: {ex.Message}");
        }
    }

    // ── Envío simple (texto / HTML, sin adjuntos) ─────────────────────────

    private async Task<EmailSendResult> SendSimpleAsync(
        EmailMessage message, CancellationToken ct)
    {
        var request = new SesV2Model.SendEmailRequest
        {
            FromEmailAddress = FormatAddress(message.From),
            Destination = BuildDestination(message),
            Content = new SesV2Model.EmailContent
            {
                Simple = new SesV2Model.Message
                {
                    Subject = new SesV2Model.Content
                    { Data = message.Subject, Charset = "UTF-8" },
                    Body = new SesV2Model.Body
                    {
                        Text = message.TextBody is not null
                            ? new SesV2Model.Content
                            { Data = message.TextBody, Charset = "UTF-8" }
                            : null,
                        Html = message.HtmlBody is not null
                            ? new SesV2Model.Content
                            { Data = message.HtmlBody, Charset = "UTF-8" }
                            : null
                    }
                }
            }
        };

        var response = await _ses.SendEmailAsync(request, ct);
        return new EmailSendResult(Success: true, ProviderMessageId: response.MessageId);
    }

    // ── Envío con template SES ────────────────────────────────────────────

    private async Task<EmailSendResult> SendWithTemplateAsync(
        EmailMessage message, CancellationToken ct)
    {
        var templateData = System.Text.Json.JsonSerializer.Serialize(
            message.Template!.Variables);

        var request = new SesV2Model.SendEmailRequest
        {
            FromEmailAddress = FormatAddress(message.From),
            Destination = BuildDestination(message),
            Content = new SesV2Model.EmailContent
            {
                Template = new SesV2Model.Template
                {
                    TemplateName = message.Template.TemplateKey,
                    TemplateData = templateData
                }
            }
        };

        var response = await _ses.SendEmailAsync(request, ct);
        return new EmailSendResult(Success: true, ProviderMessageId: response.MessageId);
    }

    // ── Envío raw MIME (con adjuntos) ─────────────────────────────────────

    private async Task<EmailSendResult> SendRawAsync(
        EmailMessage message, CancellationToken ct)
    {
        var request = new SesV2Model.SendEmailRequest
        {
            FromEmailAddress = FormatAddress(message.From),
            Destination = BuildDestination(message),
            Content = new SesV2Model.EmailContent
            {
                Raw = new SesV2Model.RawMessage
                { Data = BuildMimeMessage(message) }
            }
        };

        var response = await _ses.SendEmailAsync(request, ct);
        return new EmailSendResult(Success: true, ProviderMessageId: response.MessageId);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static SesV2Model.Destination BuildDestination(EmailMessage message)
        => new()
        {
            ToAddresses = message.To.Select(FormatAddress).ToList(),
            CcAddresses = message.Cc.Select(FormatAddress).ToList(),
            BccAddresses = message.Bcc.Select(FormatAddress).ToList()
        };

    private static string FormatAddress(EmailRecipient r)
        => r.Name is not null ? $"{r.Name} <{r.Address}>" : r.Address;

    private static MemoryStream BuildMimeMessage(EmailMessage message)
    {
        var boundary = $"----=_Part_{Guid.NewGuid():N}";
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"From: {FormatAddress(message.From)}");
        sb.AppendLine($"To: {string.Join(", ", message.To.Select(FormatAddress))}");
        if (message.Cc.Count > 0)
            sb.AppendLine($"Cc: {string.Join(", ", message.Cc.Select(FormatAddress))}");
        sb.AppendLine($"Subject: {message.Subject}");
        sb.AppendLine("MIME-Version: 1.0");
        sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        sb.AppendLine();

        sb.AppendLine($"--{boundary}");
        if (message.HtmlBody is not null)
        {
            sb.AppendLine("Content-Type: text/html; charset=UTF-8");
            sb.AppendLine("Content-Transfer-Encoding: quoted-printable");
            sb.AppendLine();
            sb.AppendLine(message.HtmlBody);
        }
        else if (message.TextBody is not null)
        {
            sb.AppendLine("Content-Type: text/plain; charset=UTF-8");
            sb.AppendLine();
            sb.AppendLine(message.TextBody);
        }

        foreach (var att in message.Attachments)
        {
            sb.AppendLine($"--{boundary}");
            sb.AppendLine($"Content-Type: {att.ContentType}; name=\"{att.FileName}\"");
            sb.AppendLine("Content-Transfer-Encoding: base64");
            sb.AppendLine($"Content-Disposition: attachment; filename=\"{att.FileName}\"");
            sb.AppendLine();
            sb.AppendLine(Convert.ToBase64String(att.Content));
        }

        sb.AppendLine($"--{boundary}--");

        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
    }
}