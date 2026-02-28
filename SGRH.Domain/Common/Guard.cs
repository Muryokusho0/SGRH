using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Common;

namespace SGRH.Domain.Common;

public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} no puede estar vacío.");

        if (value.Length > maxLength)
            throw new DomainException($"{name} supera el máximo permitido ({maxLength}).");
    }

    public static void AgainstOutOfRange(int value, string name, int minExclusive)
    {
        if (value <= minExclusive)
            throw new DomainException($"{name} debe ser > {minExclusive}.");
    }

    public static void AgainstOutOfRange(decimal value, string name, decimal minExclusive)
    {
        if (value <= minExclusive)
            throw new DomainException($"{name} debe ser > {minExclusive}.");
    }

    public static void AgainstInvalidDateRange(DateTime start, DateTime end, string startName, string endName)
    {
        if (start >= end)
            throw new DomainException($"{startName} debe ser menor que {endName}.");
    }

    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new DomainException($"{name} no puede ser null.");
    }
}
