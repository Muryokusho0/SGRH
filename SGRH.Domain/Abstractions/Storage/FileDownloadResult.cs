using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public sealed record FileDownloadResult(
    bool Success,
    string? ContentType = null,
    byte[]? Content = null,
    string? Error = null);
