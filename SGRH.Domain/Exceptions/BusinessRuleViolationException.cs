using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una operación viola una regla de negocio del dominio.
/// Por ejemplo: intentar confirmar una reserva sin habitaciones, o cambiar el estado
/// de una habitación al mismo estado actual.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="BusinessRuleViolationException"/>
    /// con el mensaje descriptivo de la regla de negocio que fue violada.
    /// </summary>
    /// <param name="message">Descripción de la regla de negocio infringida.</param>
    public BusinessRuleViolationException(string message)
        : base("BUSINESS_RULE_VIOLATION", message) { }
}