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
    public decimal TarifaAplicada { get; private set; } // snapshot

    private DetalleReserva() { }

    public DetalleReserva(int reservaId, int habitacionId, decimal tarifaAplicada)
    {
        Guard.AgainstOutOfRange(reservaId, nameof(reservaId), 0);
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);
        Guard.AgainstOutOfRange(tarifaAplicada, nameof(tarifaAplicada), 0);

        ReservaId = reservaId;
        HabitacionId = habitacionId;
        TarifaAplicada = tarifaAplicada;
    }

    // Solo el agregado Reserva debería ajustar snapshots (por repricing en Pendiente)
    internal void ActualizarTarifa(decimal nuevaTarifa)
    {
        Guard.AgainstOutOfRange(nuevaTarifa, nameof(nuevaTarifa), 0);
        TarifaAplicada = nuevaTarifa;
    }

    protected override object GetKey() => DetalleReservaId == 0
        ? $"{ReservaId}-{HabitacionId}"
        : DetalleReservaId;
}
