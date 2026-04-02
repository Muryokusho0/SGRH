using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una operación genera un conflicto de integridad o duplicidad.
/// Por ejemplo: agregar una habitación que ya está en la reserva, o habilitar un servicio
/// en una temporada donde ya está registrado.
/// </summary>
public sealed class ConflictException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ConflictException"/>
    /// con el mensaje descriptivo del conflicto detectado.
    /// </summary>
    /// <param name="message">Descripción del conflicto ocurrido.</param>
    public ConflictException(string message)
        : base("CONFLICT", message) { }
}