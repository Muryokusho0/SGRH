using SGRH.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

/// <summary>
/// Objeto de valor que representa una ruta normalizada dentro del servicio de almacenamiento.
/// Garantiza que la ruta use barras diagonales (/) y no tenga slash inicial.
/// </summary>
public sealed class StoragePath
{
    /// <summary>
    /// Valor de la ruta normalizada en formato de barras diagonales, sin slash inicial.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Crea una nueva ruta de almacenamiento normalizando el valor proporcionado.
    /// Convierte backslashes en forward slashes y elimina el slash inicial si existe.
    /// </summary>
    /// <param name="value">Ruta en el sistema de almacenamiento (máx. 1024 caracteres).</param>
    /// <exception cref="Exceptions.ValidationException">Si el valor es nulo, vacío o supera la longitud máxima.</exception>
    public StoragePath(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value), 1024);
        // Normalizar: backslashes → forward slash, sin slash al inicio
        Value = value.Replace("\\", "/").TrimStart('/');
    }

    /// <summary>
    /// Devuelve la representación en cadena de la ruta de almacenamiento.
    /// </summary>
    /// <returns>El valor normalizado de la ruta.</returns>
    public override string ToString() => Value;
}