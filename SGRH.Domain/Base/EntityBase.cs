using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Base;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Implementa igualdad basada en clave de negocio en lugar de referencia de objeto.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Devuelve la clave de negocio única de la entidad,
    /// usada para comparar igualdad entre instancias.
    /// </summary>
    /// <returns>Objeto que representa la clave primaria de la entidad.</returns>
    protected abstract object GetKey();

    /// <summary>
    /// Determina si el objeto especificado es igual a la instancia actual,
    /// comparando por tipo y clave de negocio.
    /// </summary>
    /// <param name="obj">Objeto a comparar con la instancia actual.</param>
    /// <returns><c>true</c> si los objetos son del mismo tipo y tienen la misma clave; de lo contrario, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return GetKey().Equals(other.GetKey());
    }

    /// <summary>
    /// Devuelve el código hash basado en la clave de negocio de la entidad.
    /// </summary>
    /// <returns>Código hash entero derivado de la clave primaria.</returns>
    public override int GetHashCode() => GetKey().GetHashCode();

    /// <summary>
    /// Operador de igualdad entre dos entidades del dominio.
    /// </summary>
    /// <param name="a">Primera entidad a comparar.</param>
    /// <param name="b">Segunda entidad a comparar.</param>
    /// <returns><c>true</c> si ambas entidades son iguales o ambas son <c>null</c>.</returns>
    public static bool operator ==(EntityBase? a, EntityBase? b)
        => a is null ? b is null : a.Equals(b);

    /// <summary>
    /// Operador de desigualdad entre dos entidades del dominio.
    /// </summary>
    /// <param name="a">Primera entidad a comparar.</param>
    /// <param name="b">Segunda entidad a comparar.</param>
    /// <returns><c>true</c> si las entidades son diferentes.</returns>
    public static bool operator !=(EntityBase? a, EntityBase? b)
        => !(a == b);
}