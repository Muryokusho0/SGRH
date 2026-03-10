using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(
        EmailMessage message, CancellationToken ct = default);
}