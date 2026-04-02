using SGRH.Domain.Base;
using SGRH.Domain.Common;
using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Entities.Habitaciones;

/// <summary>
/// Define una categoría de habitación del hotel (por ejemplo: Suite, Doble, Simple).
/// Agrupa habitaciones con características similares y establece un precio base para la tarifa.
/// </summary>
public sealed class CategoriaHabitacion : EntityBase
{
    /// <summary>
    /// Identificador único de la categoría de habitación.
    /// </summary>
    public int CategoriaHabitacionId { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la categoría (por ejemplo: "Suite Presidencial", "Habitación Doble").
    /// </summary>
    public string NombreCategoria { get; private set; } = default!;

    /// <summary>
    /// Número máximo de personas que puede alojar una habitación de esta categoría.
    /// </summary>
    public int Capacidad { get; private set; }

    /// <summary>
    /// Descripción detallada de las comodidades y características de la categoría.
    /// </summary>
    public string Descripcion { get; private set; } = default!;

    /// <summary>
    /// Precio base por noche de las habitaciones de esta categoría,
    /// utilizado cuando no existe una tarifa de temporada aplicable.
    /// </summary>
    public decimal PrecioBase { get; private set; }

    private CategoriaHabitacion() { }

    /// <summary>
    /// Crea una nueva categoría de habitación con su nombre, capacidad, descripción y precio base.
    /// </summary>
    /// <param name="nombreCategoria">Nombre de la categoría (máx. 50 caracteres).</param>
    /// <param name="capacidad">Capacidad máxima de huéspedes (mayor a 0).</param>
    /// <param name="descripcion">Descripción de la categoría (máx. 255 caracteres).</param>
    /// <param name="precioBase">Precio base por noche en la moneda local (mayor a 0).</param>
    public CategoriaHabitacion(
        string nombreCategoria,
        int capacidad,
        string descripcion,
        decimal precioBase)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCategoria, nameof(nombreCategoria), 50);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 255);
        Guard.AgainstOutOfRange(capacidad, nameof(capacidad), 0);
        Guard.AgainstOutOfRange(precioBase, nameof(precioBase), 0m);

        NombreCategoria = nombreCategoria;
        Capacidad = capacidad;
        Descripcion = descripcion;
        PrecioBase = precioBase;
    }

    /// <summary>
    /// Actualiza los datos de la categoría de habitación con nuevos valores.
    /// </summary>
    /// <param name="nombreCategoria">Nuevo nombre de la categoría (máx. 50 caracteres).</param>
    /// <param name="capacidad">Nueva capacidad máxima (mayor a 0).</param>
    /// <param name="descripcion">Nueva descripción de la categoría (máx. 255 caracteres).</param>
    /// <param name="precioBase">Nuevo precio base por noche (mayor a 0).</param>
    public void Actualizar(
        string nombreCategoria,
        int capacidad,
        string descripcion,
        decimal precioBase)
    {
        Guard.AgainstNullOrWhiteSpace(nombreCategoria, nameof(nombreCategoria), 50);
        Guard.AgainstNullOrWhiteSpace(descripcion, nameof(descripcion), 255);
        Guard.AgainstOutOfRange(capacidad, nameof(capacidad), 0);
        Guard.AgainstOutOfRange(precioBase, nameof(precioBase), 0m);

        NombreCategoria = nombreCategoria;
        Capacidad = capacidad;
        Descripcion = descripcion;
        PrecioBase = precioBase;
    }

    protected override object GetKey() => CategoriaHabitacionId;
}

