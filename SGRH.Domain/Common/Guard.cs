using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Abstractions.Policies;
using SGRH.Domain.Common;
using SGRH.Domain.Entities.Reservas;
using SGRH.Domain.Exceptions;

namespace SGRH.Domain.Common;

public static class Guard
{

    public static void AgainstNullOrWhiteSpace(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} no puede estar vacío.");

        if (value.Length > maxLength)
            throw new ValidationException($"{name} supera el máximo de {maxLength} caracteres.");
    }

    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new ValidationException($"{name} no puede ser null.");
    }

    public static void AgainstOutOfRange(int value, string name, int minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    public static void AgainstOutOfRange(long value, string name, long minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    public static void AgainstOutOfRange(decimal value, string name, decimal minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    public static void AgainstInvalidDateRange(DateTime start, DateTime end,
                                               string startName, string endName)
    {
        if (start >= end)
            throw new ValidationException($"{startName} debe ser anterior a {endName}.");
    }
}
