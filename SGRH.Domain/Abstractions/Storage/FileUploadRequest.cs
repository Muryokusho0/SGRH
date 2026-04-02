using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Encapsula la información necesaria para cargar un archivo al servicio de almacenamiento.
/// Incluye la ruta de destino, el tipo MIME y el contenido binario del archivo.
/// </summary>
public sealed class FileUploadRequest
{
    /// <summary>
    /// Ruta de destino en el servicio de almacenamiento donde se guardará el archivo.
    /// </summary>
    public StoragePath Path { get; }

    /// <summary>
    /// Tipo MIME del archivo a cargar (por ejemplo: "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Contenido binario del archivo a cargar.
    /// </summary>
    public byte[] Content { get; }

    /// <summary>
    /// Crea una nueva solicitud de carga de archivo con la ruta, tipo y contenido indicados.
    /// </summary>
    /// <param name="path">Ruta de destino en el almacenamiento.</param>
    /// <param name="contentType">Tipo MIME del archivo (máx. 100 caracteres).</param>
    /// <param name="content">Contenido binario del archivo (no puede estar vacío).</param>
    /// <exception cref="Exceptions.ValidationException">Si el contenido está vacío.</exception>
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