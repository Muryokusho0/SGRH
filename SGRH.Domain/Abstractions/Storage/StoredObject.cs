using SGRH.Domain.Common;
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
    public string? ETag { get; }             // Hash de integridad que devuelve S3
    public DateTimeOffset? LastModifiedUtc { get; }

    public StoredObject(
        StoragePath path,
        string contentType,
        long sizeBytes,
        string? eTag = null,
        DateTimeOffset? lastModifiedUtc = null)
    {
        Guard.AgainstNull(path, nameof(path));
        Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 100);
        Guard.AgainstOutOfRange(sizeBytes, nameof(sizeBytes), 0L);

        Path = path;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        ETag = eTag;
        LastModifiedUtc = lastModifiedUtc;
    }
}