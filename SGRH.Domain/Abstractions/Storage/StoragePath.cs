using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public sealed class StoragePath
{
    public string Value { get; }

    public StoragePath(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value), 1024);
        // Normalizar: backslashes → forward slash, sin slash al inicio
        Value = value.Replace("\\", "/").TrimStart('/');
    }

    public override string ToString() => Value;
}