using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public sealed record FileUploadResult(
    bool Success,
    StoredObject? StoredObject = null,
    string? Error = null);
