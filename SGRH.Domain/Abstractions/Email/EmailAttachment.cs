using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Email;

/// <summary>
/// Representa un archivo adjunto que se incluye en un mensaje de correo electrónico.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// Nombre del archivo adjunto, incluyendo su extensión (por ejemplo: "reporte.pdf").
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Tipo MIME del archivo adjunto (por ejemplo: "application/pdf", "image/png").
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Contenido binario del archivo adjunto.
    /// </summary>
    public byte[] Content { get; }

    /// <summary>
    /// Crea un nuevo adjunto de correo electrónico con el nombre, tipo y contenido indicados.
    /// </summary>
    /// <param name="fileName">Nombre del archivo adjunto (máx. 255 caracteres).</param>
    /// <param name="contentType">Tipo MIME del archivo (máx. 100 caracteres).</param>
    /// <param name="content">Contenido binario del archivo (no puede estar vacío).</param>
    /// <exception cref="Exceptions.ValidationException">Si el contenido está vacío.</exception>
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
