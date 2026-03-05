using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Email;

public sealed class EmailAttachment
{
    public string FileName { get; }
    public string ContentType { get; }
    public byte[] Content { get; }

    public EmailAttachment(string fileName, string contentType, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName es requerido.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("ContentType es requerido.", nameof(contentType));
        Content = content ?? throw new ArgumentNullException(nameof(content));

        FileName = fileName;
        ContentType = contentType;
    }
}
