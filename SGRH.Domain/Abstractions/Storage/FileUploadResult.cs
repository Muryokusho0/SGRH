using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Resultado de un intento de carga de archivo al servicio de almacenamiento.
/// </summary>
/// <param name="Success">Indica si la carga fue exitosa.</param>
/// <param name="StoredObject">
/// Metadatos del objeto almacenado (ruta, tipo, tamaño, ETag).
/// Es <c>null</c> si la carga falló.
/// </param>
/// <param name="Error">
/// Mensaje de error descriptivo si la carga falló. Es <c>null</c> si fue exitosa.
/// </param>
public sealed record FileUploadResult(
    bool Success,
    StoredObject? StoredObject = null,
    string? Error = null);
