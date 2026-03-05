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
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("StoragePath es requerido.", nameof(value));

        Value = value.Replace("\\", "/").TrimStart('/');
    }

    public override string ToString()
    {
        return Value;
    }
}