using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Reservas;

public sealed class ReservaServicioAdicional : EntityBase
{
    public int ReservaServicioAdicionalId { get; private set; }
    public int ReservaId { get; private set; }
    public int ServicioAdicionalId { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitarioAplicado { get; private set; }
    public decimal SubTotal => Cantidad * PrecioUnitarioAplicado;

    private ReservaServicioAdicional() { }

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

    internal void CambiarCantidad(int nuevaCantidad)
    {
        Guard.AgainstOutOfRange(nuevaCantidad, nameof(nuevaCantidad), 0);
        Cantidad = nuevaCantidad;
    }

    internal void ActualizarPrecioUnitario(decimal nuevoPrecio)
    {
        Guard.AgainstOutOfRange(nuevoPrecio, nameof(nuevoPrecio), 0m);
        PrecioUnitarioAplicado = nuevoPrecio;
    }

    protected override object GetKey() => ReservaServicioAdicionalId;
}