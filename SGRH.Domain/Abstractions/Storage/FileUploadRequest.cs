using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Storage;

public sealed class FileUploadRequest
{
    public StoragePath Path { get; }
    public string ContentType { get; }
    public byte[] Content { get; }

    public FileUploadRequest(StoragePath path, string contentType, byte[] content)
    {
        Guard.AgainstNull(path, nameof(path));
        Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 100);
        Guard.AgainstNull(content, nameof(content));

        if (content.Length == 0)
            throw new ValidationException("El contenido del archivo no puede estar vacío.");

        Path = path;
        ContentType = contentType;
        Content = content;
    }
}