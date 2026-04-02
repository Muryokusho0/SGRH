using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Resultado de un intento de descarga de archivo desde el servicio de almacenamiento.
/// </summary>
/// <param name="Success">Indica si la descarga fue exitosa.</param>
/// <param name="ContentType">
/// Tipo MIME del archivo descargado (por ejemplo: "application/pdf").
/// Es <c>null</c> si la descarga falló.
/// </param>
/// <param name="Content">
/// Contenido binario del archivo descargado. Es <c>null</c> si la descarga falló.
/// </param>
/// <param name="Error">
/// Mensaje de error descriptivo si la descarga falló. Es <c>null</c> si fue exitosa.
/// </param>
public sealed record FileDownloadResult(
    bool Success,
    string? ContentType = null,
    byte[]? Content = null,
    string? Error = null);
