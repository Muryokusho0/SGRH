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

/// <summary>
/// Clase de utilidad estática que agrupa validaciones de guardia (guard clauses)
/// para proteger invariantes del dominio lanzando <see cref="Exceptions.ValidationException"/>
/// cuando los valores no cumplen las restricciones esperadas.
/// </summary>
public static class Guard
{

    /// <summary>
    /// Verifica que una cadena no sea nula, vacía ni solo espacios en blanco,
    /// y que no supere la longitud máxima indicada.
    /// </summary>
    /// <param name="value">Valor de cadena a validar.</param>
    /// <param name="name">Nombre del campo, usado en el mensaje de error.</param>
    /// <param name="maxLength">Longitud máxima permitida en caracteres.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si el valor es nulo/vacío o supera <paramref name="maxLength"/> caracteres.
    /// </exception>
    public static void AgainstNullOrWhiteSpace(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} no puede estar vacío.");

        if (value.Length > maxLength)
            throw new ValidationException($"{name} supera el máximo de {maxLength} caracteres.");
    }

    /// <summary>
    /// Verifica que un objeto no sea <c>null</c>.
    /// </summary>
    /// <param name="value">Valor a verificar.</param>
    /// <param name="name">Nombre del campo, usado en el mensaje de error.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si el valor es <c>null</c>.
    /// </exception>
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new ValidationException($"{name} no puede ser null.");
    }

    /// <summary>
    /// Verifica que un valor entero sea estrictamente mayor al mínimo indicado.
    /// </summary>
    /// <param name="value">Valor entero a validar.</param>
    /// <param name="name">Nombre del campo, usado en el mensaje de error.</param>
    /// <param name="minExclusive">Límite inferior exclusivo que el valor debe superar.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si <paramref name="value"/> es menor o igual a <paramref name="minExclusive"/>.
    /// </exception>
    public static void AgainstOutOfRange(int value, string name, int minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    /// <summary>
    /// Verifica que un valor <see cref="long"/> sea estrictamente mayor al mínimo indicado.
    /// </summary>
    /// <param name="value">Valor entero largo a validar.</param>
    /// <param name="name">Nombre del campo, usado en el mensaje de error.</param>
    /// <param name="minExclusive">Límite inferior exclusivo que el valor debe superar.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si <paramref name="value"/> es menor o igual a <paramref name="minExclusive"/>.
    /// </exception>
    public static void AgainstOutOfRange(long value, string name, long minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    /// <summary>
    /// Verifica que un valor decimal sea estrictamente mayor al mínimo indicado.
    /// </summary>
    /// <param name="value">Valor decimal a validar.</param>
    /// <param name="name">Nombre del campo, usado en el mensaje de error.</param>
    /// <param name="minExclusive">Límite inferior exclusivo que el valor debe superar.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si <paramref name="value"/> es menor o igual a <paramref name="minExclusive"/>.
    /// </exception>
    public static void AgainstOutOfRange(decimal value, string name, decimal minExclusive)
    {
        if (value <= minExclusive)
            throw new ValidationException($"{name} debe ser mayor a {minExclusive}.");
    }

    /// <summary>
    /// Verifica que una fecha de inicio sea estrictamente anterior a la fecha de fin.
    /// </summary>
    /// <param name="start">Fecha de inicio del rango.</param>
    /// <param name="end">Fecha de fin del rango.</param>
    /// <param name="startName">Nombre del campo de inicio, usado en el mensaje de error.</param>
    /// <param name="endName">Nombre del campo de fin, usado en el mensaje de error.</param>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si <paramref name="start"/> es mayor o igual a <paramref name="end"/>.
    /// </exception>
    public static void AgainstInvalidDateRange(DateTime start, DateTime end,
                                               string startName, string endName)
    {
        if (start >= end)
            throw new ValidationException($"{startName} debe ser anterior a {endName}.");
    }
}
