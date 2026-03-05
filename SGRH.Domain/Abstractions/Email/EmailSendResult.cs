using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

public sealed record EmailSendResult(
    bool Success,
    string? ProviderMessageId = null,
    string? Error = null);
