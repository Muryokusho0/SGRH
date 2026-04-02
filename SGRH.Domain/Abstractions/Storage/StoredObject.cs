using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Representa los metadatos de un archivo almacenado exitosamente en el servicio de almacenamiento.
/// Contiene la información retornada por el proveedor tras una carga exitosa.
/// </summary>
public sealed class StoredObject
{
    /// <summary>
    /// Ruta del archivo dentro del servicio de almacenamiento.
    /// </summary>
    public StoragePath Path { get; }

    /// <summary>
    /// Tipo MIME del archivo almacenado (por ejemplo: "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Tamaño del archivo en bytes.
    /// </summary>
    public long SizeBytes { get; }

    /// <summary>
    /// Hash de integridad (ETag) devuelto por el proveedor de almacenamiento (por ejemplo, Amazon S3).
    /// Puede ser <c>null</c> si el proveedor no lo devuelve.
    /// </summary>
    public string? ETag { get; }

    /// <summary>
    /// Fecha y hora de la última modificación del archivo en el servicio de almacenamiento.
    /// Puede ser <c>null</c> si el proveedor no proporciona este dato.
    /// </summary>
    public DateTimeOffset? LastModifiedUtc { get; }

    /// <summary>
    /// Crea una nueva instancia de metadatos de archivo almacenado.
    /// </summary>
    /// <param name="path">Ruta del archivo en el almacenamiento.</param>
    /// <param name="contentType">Tipo MIME del archivo (máx. 100 caracteres).</param>
    /// <param name="sizeBytes">Tamaño del archivo en bytes (mayor a 0).</param>
    /// <param name="eTag">Hash de integridad devuelto por el proveedor (opcional).</param>
    /// <param name="lastModifiedUtc">Fecha de última modificación en UTC (opcional).</param>
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