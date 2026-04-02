using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando uno o más valores de entrada no pasan las validaciones
/// de dominio definidas en <see cref="Common.Guard"/> u otras reglas de invariantes.
/// </summary>
public sealed class ValidationException : DomainException
{
    /// <summary>
    /// Lista de mensajes de error de validación individuales.
    /// Puede contener uno o varios errores cuando se validan múltiples campos a la vez.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ValidationException"/> con un único mensaje de error.
    /// </summary>
    /// <param name="error">Mensaje descriptivo del error de validación.</param>
    public ValidationException(string error)
        : base("VALIDATION_ERROR", error)
    {
        Errors = string.IsNullOrWhiteSpace(error) ? [] : [error];
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ValidationException"/> con múltiples mensajes de error.
    /// </summary>
    /// <param name="errors">Colección de mensajes de error de validación. Se descartan los valores vacíos.</param>
    public ValidationException(IEnumerable<string> errors)
        : base("VALIDATION_ERROR", "Hay errores de validación.")
    {
        Errors = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? [];
    }
}
