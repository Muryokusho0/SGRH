using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;


namespace SGRH.Domain.Entities.Habitaciones;

/// <summary>
/// Define el precio por noche de una categoría de habitación durante una temporada específica.
/// Sobreescribe el precio base de la categoría para el período de la temporada activa.
/// </summary>
public sealed class TarifaTemporada : EntityBase
{
    /// <summary>
    /// Identificador único de la tarifa de temporada.
    /// </summary>
    public int TarifaTemporadaId { get; private set; }

    /// <summary>
    /// Identificador de la categoría de habitación a la que aplica esta tarifa.
    /// </summary>
    public int CategoriaHabitacionId { get; private set; }

    /// <summary>
    /// Identificador de la temporada durante la cual rige esta tarifa.
    /// </summary>
    public int TemporadaId { get; private set; }

    /// <summary>
    /// Precio por noche en la moneda local para la categoría y temporada indicadas.
    /// </summary>
    public decimal Precio { get; private set; }

    private TarifaTemporada() { }

    /// <summary>
    /// Crea una nueva tarifa de temporada para una categoría de habitación.
    /// </summary>
    /// <param name="categoriaHabitacionId">Id de la categoría de habitación (mayor a 0).</param>
    /// <param name="temporadaId">Id de la temporada (mayor a 0).</param>
    /// <param name="precio">Precio por noche aplicable (mayor a 0).</param>
    public TarifaTemporada(int categoriaHabitacionId, int temporadaId, decimal precio)
    {
        Guard.AgainstOutOfRange(categoriaHabitacionId, nameof(categoriaHabitacionId), 0);
        Guard.AgainstOutOfRange(temporadaId, nameof(temporadaId), 0);
        Guard.AgainstOutOfRange(precio, nameof(precio), 0m);

        CategoriaHabitacionId = categoriaHabitacionId;
        TemporadaId = temporadaId;
        Precio = precio;
    }

    /// <summary>
    /// Actualiza el precio de la tarifa de temporada con un nuevo valor.
    /// </summary>
    /// <param name="nuevoPrecio">Nuevo precio por noche (mayor a 0).</param>
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        Precio = nuevoPrecio;
    }

    protected override object GetKey() => TarifaTemporadaId;
}
