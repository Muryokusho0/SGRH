using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Reservas;

/// <summary>
/// Representa un servicio adicional incluido en una reserva, con la cantidad solicitada
/// y el snapshot del precio unitario congelado al momento de agregarlo.
/// Solo puede ser creado desde la entidad <see cref="Reserva"/>.
/// </summary>
public sealed class ReservaServicioAdicional : EntityBase
{
    /// <summary>
    /// Identificador único de la asociación entre reserva y servicio adicional.
    /// </summary>
    public int ReservaServicioAdicionalId { get; private set; }

    /// <summary>
    /// Identificador de la reserva a la que pertenece este servicio.
    /// </summary>
    public int ReservaId { get; private set; }

    /// <summary>
    /// Identificador del servicio adicional contratado.
    /// </summary>
    public int ServicioAdicionalId { get; private set; }

    /// <summary>
    /// Cantidad de unidades del servicio solicitadas en la reserva.
    /// </summary>
    public int Cantidad { get; private set; }

    /// <summary>
    /// Precio unitario del servicio (snapshot) congelado al momento de agregarlo a la reserva.
    /// </summary>
    public decimal PrecioUnitarioAplicado { get; private set; }

    /// <summary>
    /// Subtotal calculado como <c>Cantidad</c> multiplicado por <c>PrecioUnitarioAplicado</c>.
    /// </summary>
    public decimal SubTotal => Cantidad * PrecioUnitarioAplicado;

    private ReservaServicioAdicional() { }

    /// <summary>
    /// Crea una nueva asociación entre una reserva y un servicio adicional con cantidad y precio congelados.
    /// Solo puede ser instanciado dentro del dominio.
    /// </summary>
    /// <param name="reservaId">Id de la reserva padre.</param>
    /// <param name="servicioAdicionalId">Id del servicio adicional (mayor a 0).</param>
    /// <param name="cantidad">Cantidad de unidades del servicio (mayor a 0).</param>
    /// <param name="precioUnitarioAplicado">Precio unitario congelado al momento de la reserva (mayor a 0).</param>
    internal ReservaServicioAdicional(
        int reservaId,
        int servicioAdicionalId,
        int cantidad,
        decimal precioUnitarioAplicado)
    {
        Guard.AgainstOutOfRange(servicioAdicionalId, nameof(servicioAdicionalId), 0);
        Guard.AgainstOutOfRange(cantidad, nameof(cantidad), 0);
        Guard.AgainstOutOfRange(precioUnitarioAplicado, nameof(precioUnitarioAplicado), 0m);

        ReservaId = reservaId;
        ServicioAdicionalId = servicioAdicionalId;
        Cantidad = cantidad;
        PrecioUnitarioAplicado = precioUnitarioAplicado;
    }

    /// <summary>
    /// Modifica la cantidad de unidades del servicio contratado en la reserva.
    /// Solo puede ser llamado desde <see cref="Reserva"/>.
    /// </summary>
    /// <param name="nuevaCantidad">Nueva cantidad de unidades (mayor a 0).</param>
    internal void CambiarCantidad(int nuevaCantidad)
    {
        Guard.AgainstOutOfRange(nuevaCantidad, nameof(nuevaCantidad), 0);
        Cantidad = nuevaCantidad;
    }

    /// <summary>
    /// Actualiza el precio unitario del servicio cuando se recalculan los snapshots de la reserva.
    /// Solo puede ser llamado desde <see cref="Reserva"/>.
    /// </summary>
    /// <param name="nuevoPrecio">Nuevo precio unitario (mayor a 0).</param>
    internal void ActualizarPrecioUnitario(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        PrecioUnitarioAplicado = nuevoPrecio;
    }

    protected override object GetKey() => ReservaServicioAdicionalId;
}