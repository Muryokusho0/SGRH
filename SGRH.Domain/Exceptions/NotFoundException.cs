using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public string EntityName { get; }
    public string? EntityId { get; }

    public NotFoundException(string entityName, string? entityId = null)
        : base("NOT_FOUND", BuildMessage(entityName, entityId))
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    private static string BuildMessage(string entityName, string? entityId)
        => entityId is null
            ? $"{entityName} no encontrado."
            : $"{entityName} no encontrado. Id={entityId}.";
}
