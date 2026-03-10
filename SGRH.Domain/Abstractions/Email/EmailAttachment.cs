using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Email;

public sealed class EmailAttachment
{
    public string FileName { get; }
    public string ContentType { get; }
    public byte[] Content { get; }

    public EmailAttachment(string fileName, string contentType, byte[] content)
    {
        Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 255);
        Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 100);
        Guard.AgainstNull(content, nameof(content));

        if (content.Length == 0)
            throw new ValidationException("El contenido del adjunto no puede estar vacío.");

        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }
}
