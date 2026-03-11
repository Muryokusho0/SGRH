using Microsoft.Extensions.Options;
using SGRH.Domain.Abstractions.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.EmailSES;

public sealed class SesAdminNotifier : IAdminNotifier
{
    private readonly IEmailSender _sender;
    private readonly SesOptions _options;

    public SesAdminNotifier(IEmailSender sender, IOptions<SesOptions> options)
    {
        _sender = sender;
        _options = options.Value;
    }

    public Task NotifyErrorAsync(
        Exception exception,
        AdminNotificationContext? context = null,
        CancellationToken ct = default)
    {
        var subject = $"[SGRH ERROR] {exception.GetType().Name}: {Truncate(exception.Message, 100)}";
        var body = BuildBody("ERROR", exception.ToString(), context);
        return SendAsync(subject, body, ct);
    }

    public Task NotifyWarningAsync(
        string mensaje,
        AdminNotificationContext? context = null,
        CancellationToken ct = default)
    {
        var subject = $"[SGRH WARNING] {Truncate(mensaje, 120)}";
        var body = BuildBody("WARNING", mensaje, context);
        return SendAsync(subject, body, ct);
    }

    private Task SendAsync(string subject, string htmlBody, CancellationToken ct)
    {
        var from = new EmailRecipient(_options.FromAddress, _options.FromName);
        var to = new[] { new EmailRecipient(_options.AdminEmail) };
        var email = new EmailMessage(from, to, subject, htmlBody: htmlBody);
        return _sender.SendAsync(email, ct);
    }

    private static string BuildBody(string level, string detail, AdminNotificationContext? ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<h2>[{level}] SGRH Notification</h2>");
        sb.AppendLine($"<p><strong>Timestamp (UTC):</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>");

        if (ctx is not null)
        {
            if (ctx.RequestId.HasValue)
                sb.AppendLine($"<p><strong>RequestId:</strong> {ctx.RequestId}</p>");
            if (ctx.Endpoint is not null)
                sb.AppendLine($"<p><strong>Endpoint:</strong> {ctx.Endpoint}</p>");
            if (ctx.Username is not null)
                sb.AppendLine($"<p><strong>Usuario:</strong> {ctx.Username}</p>");
            if (ctx.IpOrigen is not null)
                sb.AppendLine($"<p><strong>IP:</strong> {ctx.IpOrigen}</p>");
            if (ctx.Extra?.Count > 0)
            {
                sb.AppendLine("<p><strong>Extra:</strong></p><ul>");
                foreach (var (k, v) in ctx.Extra)
                    sb.AppendLine($"<li>{k}: {v}</li>");
                sb.AppendLine("</ul>");
            }
        }

        sb.AppendLine($"<hr/><pre>{System.Web.HttpUtility.HtmlEncode(detail)}</pre>");
        return sb.ToString();
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "...";
}