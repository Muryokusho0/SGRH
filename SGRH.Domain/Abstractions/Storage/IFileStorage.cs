using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Define el contrato del servicio de almacenamiento de archivos del sistema.
/// La implementación reside en la capa de infraestructura (por ejemplo, Amazon S3).
/// Permite cargar, descargar, eliminar y verificar la existencia de archivos.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Carga un archivo al servicio de almacenamiento de forma asíncrona.
    /// </summary>
    /// <param name="request">Solicitud de carga con ruta, tipo MIME y contenido del archivo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Un <see cref="FileUploadResult"/> indicando si la carga fue exitosa,
    /// con los metadatos del archivo almacenado o el error ocurrido.
    /// </returns>
    Task<FileUploadResult> UploadAsync(
        FileUploadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Descarga un archivo desde el servicio de almacenamiento de forma asíncrona.
    /// </summary>
    /// <param name="path">Ruta del archivo a descargar en el almacenamiento.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Un <see cref="FileDownloadResult"/> con el contenido del archivo descargado
    /// o el error ocurrido si la descarga falló.
    /// </returns>
    Task<FileDownloadResult> DownloadAsync(
        StoragePath path, CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo del servicio de almacenamiento de forma asíncrona.
    /// </summary>
    /// <param name="path">Ruta del archivo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si el archivo fue eliminado exitosamente; de lo contrario, <c>false</c>.</returns>
    Task<bool> DeleteAsync(
        StoragePath path, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un archivo existe en el servicio de almacenamiento.
    /// </summary>
    /// <param name="path">Ruta del archivo a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si el archivo existe; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        StoragePath path, CancellationToken ct = default);
}
