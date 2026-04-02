using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

/// <summary>
/// Clase base abstracta para todas las excepciones del dominio del sistema.
/// Proporciona un código identificador de error y un mensaje opcional para el usuario final.
/// </summary>
/// <remarks>
/// Todas las excepciones específicas del dominio deben heredar de esta clase
/// en lugar de <see cref="Exception"/> directamente, para garantizar consistencia
/// en el manejo de errores a nivel de API y aplicación.
/// </remarks>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Código de error único que identifica el tipo de excepción.
    /// Por ejemplo: "NOT_FOUND", "VALIDATION_ERROR", "BUSINESS_RULE_VIOLATION".
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Sugerencia opcional legible por el usuario final sobre cómo resolver el error.
    /// Puede ser <c>null</c> si no aplica.
    /// </summary>
    public string? UserHint { get; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DomainException"/> con código, mensaje y sugerencia opcionales.
    /// </summary>
    /// <param name="code">Código identificador del error. Si está vacío, se asigna "DOMAIN_ERROR".</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="userHint">Sugerencia opcional para el usuario final.</param>
    protected DomainException(string code, string message, string? userHint = null)
        : base(message)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "DOMAIN_ERROR" : code;
        UserHint = userHint;
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DomainException"/> con código, mensaje y excepción interna.
    /// </summary>
    /// <param name="code">Código identificador del error. Si está vacío, se asigna "DOMAIN_ERROR".</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="innerException">Excepción original que causó este error.</param>
    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "DOMAIN_ERROR" : code;
    }
}