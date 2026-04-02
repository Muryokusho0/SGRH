using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Base;
using SGRH.Domain.Common;

namespace SGRH.Domain.Entities.Reservas;

/// <summary>
/// Representa la asociación entre una reserva y una habitación específica,
/// incluyendo el snapshot de la tarifa aplicada al momento de agregar la habitación.
/// Solo puede ser creado e instanciado desde la entidad <see cref="Reserva"/>.
/// </summary>
public sealed class DetalleReserva : EntityBase
{
    /// <summary>
    /// Identificador único del detalle de reserva.
    /// </summary>
    public int DetalleReservaId { get; private set; }

    /// <summary>
    /// Identificador de la reserva a la que pertenece este detalle.
    /// </summary>
    public int ReservaId { get; private set; }

    /// <summary>
    /// Identificador de la habitación reservada.
    /// </summary>
    public int HabitacionId { get; private set; }

    /// <summary>
    /// Tarifa por noche (snapshot) que fue congelada al momento de agregar la habitación a la reserva.
    /// </summary>
    public decimal TarifaAplicada { get; private set; }

    private DetalleReserva() { }

    /// <summary>
    /// Crea un nuevo detalle de reserva para una habitación con su tarifa congelada.
    /// Solo puede ser instanciado dentro del dominio.
    /// </summary>
    /// <param name="reservaId">Id de la reserva padre.</param>
    /// <param name="habitacionId">Id de la habitación reservada (mayor a 0).</param>
    /// <param name="tarifaAplicada">Tarifa por noche aplicada al momento de la reserva (mayor a 0).</param>
    internal DetalleReserva(int reservaId, int habitacionId, decimal tarifaAplicada)
    {
        Guard.AgainstOutOfRange(habitacionId, nameof(habitacionId), 0);
        Guard.AgainstOutOfRange(tarifaAplicada, nameof(tarifaAplicada), 0m);

        ReservaId = reservaId;
        HabitacionId = habitacionId;
        TarifaAplicada = tarifaAplicada;
    }

    /// <summary>
    /// Actualiza el snapshot de tarifa aplicada cuando cambian las habitaciones de la reserva.
    /// Solo puede ser llamado desde <see cref="Reserva"/>.
    /// </summary>
    /// <param name="nuevaTarifa">Nueva tarifa a aplicar (mayor a 0).</param>
    internal void ActualizarTarifa(decimal nuevaTarifa)
    {
        Guard.AgainstOutOfRange(nuevaTarifa, nameof(nuevaTarifa), 0m);
        TarifaAplicada = nuevaTarifa;
    }

    protected override object GetKey() => DetalleReservaId;
}