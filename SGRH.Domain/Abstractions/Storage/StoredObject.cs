using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public sealed class StoredObject
{
    public StoragePath Path { get; }
    public string ContentType { get; }
    public long SizeBytes { get; }
    public string? ETag { get; }
    public DateTimeOffset? LastModifiedUtc { get; }

    public StoredObject(StoragePath path, string contentType, long sizeBytes, string? eTag = null, DateTimeOffset? lastModifiedUtc = null)
    {
        Path = path;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        ETag = eTag;
        LastModifiedUtc = lastModifiedUtc;
    }
}