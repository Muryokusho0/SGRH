using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public sealed class FileUploadRequest
{
    public StoragePath Path { get; }
    public string ContentType { get; }
    public byte[] Content { get; }

    public FileUploadRequest(StoragePath path, string contentType, byte[] content)
    {
        Path = path;
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? throw new ArgumentException("ContentType es requerido.", nameof(contentType))
            : contentType;

        Content = content ?? throw new ArgumentNullException(nameof(content));
        if (Content.Length == 0) throw new ArgumentException("Content no puede estar vacío.", nameof(content));
    }
}