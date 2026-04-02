using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una entidad del dominio no puede ser encontrada
/// en el repositorio o en la colección en memoria de un agregado.
/// </summary>
public sealed class NotFoundException : DomainException
{
    /// <summary>
    /// Nombre de la entidad que no fue encontrada (por ejemplo: "Reserva", "Cliente").
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Identificador de la entidad buscada. Puede ser <c>null</c> si no se especificó un id.
    /// </summary>
    public string? EntityId { get; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="NotFoundException"/>
    /// con el nombre de la entidad y, opcionalmente, su identificador.
    /// </summary>
    /// <param name="entityName">Nombre del tipo de entidad que no fue encontrada.</param>
    /// <param name="entityId">Identificador de la entidad buscada (opcional).</param>
    public NotFoundException(string entityName, string? entityId = null)
        : base("NOT_FOUND", BuildMessage(entityName, entityId))
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Construye el mensaje de error descriptivo según si se proporcionó un id o no.
    /// </summary>
    /// <param name="entityName">Nombre de la entidad.</param>
    /// <param name="entityId">Id de la entidad (opcional).</param>
    /// <returns>Cadena con el mensaje de error formateado.</returns>
    private static string BuildMessage(string entityName, string? entityId)
        => entityId is null
            ? $"{entityName} no encontrado."
            : $"{entityName} no encontrado. Id={entityId}.";
}
