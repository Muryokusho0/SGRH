using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Servicios;

/// <summary>
/// Define el precio de un servicio adicional diferenciado por categoría de habitación.
/// Permite que un mismo servicio tenga precios distintos según la categoría de la habitación
/// del huésped (por ejemplo: el precio de un masaje es mayor para una Suite que para una habitación Simple).
/// </summary>
public sealed class ServicioCategoriaPrecio : EntityBase
{
    /// <summary>
    /// Identificador del servicio adicional al que aplica este precio.
    /// </summary>
    public int ServicioAdicionalId { get; private set; }

    /// <summary>
    /// Identificador de la categoría de habitación para la que aplica este precio.
    /// </summary>
    public int CategoriaHabitacionId { get; private set; }

    /// <summary>
    /// Precio unitario del servicio para la categoría de habitación indicada.
    /// </summary>
    public decimal Precio { get; private set; }

    private ServicioCategoriaPrecio() { }

    /// <summary>
    /// Crea un nuevo precio de servicio para una categoría de habitación específica.
    /// </summary>
    /// <param name="servicioAdicionalId">Id del servicio adicional (mayor a 0).</param>
    /// <param name="categoriaHabitacionId">Id de la categoría de habitación (mayor a 0).</param>
    /// <param name="precio">Precio unitario del servicio (mayor a 0).</param>
    public ServicioCategoriaPrecio(
        int servicioAdicionalId,
        int categoriaHabitacionId,
        decimal precio)
    {
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(categoriaHabitacionId, nameof(categoriaHabitacionId), 0);
        Guard.AgainstOutOfRange(precio, nameof(precio), 0m);

        ServicioAdicionalId = servicioAdicionalId;
        CategoriaHabitacionId = categoriaHabitacionId;
        Precio = precio;
    }

    /// <summary>
    /// Actualiza el precio del servicio para la categoría de habitación asociada.
    /// </summary>
    /// <param name="nuevoPrecio">Nuevo precio unitario (mayor a 0).</param>
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        Precio = nuevoPrecio;
    }

    protected override object GetKey() => $"{ServicioAdicionalId}-{CategoriaHabitacionId}";
}