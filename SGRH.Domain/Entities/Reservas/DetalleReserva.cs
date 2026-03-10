using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Reservas;

public sealed class DetalleReserva : EntityBase
{
    public int DetalleReservaId { get; private set; }
    public int ReservaId { get; private set; }
    public int HabitacionId { get; private set; }
    public decimal TarifaAplicada { get; private set; }

    private DetalleReserva() { }

    internal DetalleReserva(int reservaId, int habitacionId, decimal tarifaAplicada)
    {
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);
        Guard.AgainstOutOfRange(tarifaAplicada, nameof(tarifaAplicada), 0m);

        ReservaId = reservaId;
        HabitacionId = habitacionId;
        TarifaAplicada = tarifaAplicada;
    }

    internal void ActualizarTarifa(decimal nuevaTarifa)
    {
        Guard.AgainstOutOfRange(nuevaTarifa, nameof(nuevaTarifa), 0m);
        TarifaAplicada = nuevaTarifa;
    }

    protected override object GetKey() => DetalleReservaId;
}