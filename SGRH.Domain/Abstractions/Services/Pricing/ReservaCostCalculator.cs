using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services.Pricing;

/// <summary>
/// Implementación concreta del calculador de costos de reservas.
/// Opera exclusivamente con los valores en memoria (snapshots congelados),
/// sin realizar consultas adicionales a la base de datos.
/// </summary>
public sealed class ReservaCostCalculator : IReservaCostCalculator
{
    /// <summary>
    /// Calcula el desglose de costos de una reserva validando los valores de entrada
    /// y sumando tarifas de habitaciones y subtotales de servicios.
    /// </summary>
    /// <param name="tarifasAplicadasHabitaciones">
    /// Lista de tarifas por noche (snapshots) de cada habitación. No puede contener valores negativos.
    /// </param>
    /// <param name="servicios">
    /// Lista de tuplas con cantidad y precio unitario de cada servicio.
    /// Las cantidades deben ser mayores a 0 y los precios no pueden ser negativos.
    /// </param>
    /// <returns>
    /// Un <see cref="ReservaCostBreakdown"/> con el total de habitaciones, servicios y reserva.
    /// </returns>
    /// <exception cref="Exceptions.ValidationException">
    /// Se lanza si alguna tarifa es negativa, alguna cantidad es menor o igual a 0,
    /// o algún precio unitario es negativo.
    /// </exception>
    public ReservaCostBreakdown Calculate(
        IReadOnlyList<decimal> tarifasAplicadasHabitaciones,
        IReadOnlyList<(int Cantidad, decimal PrecioUnitarioAplicado)> servicios)
    {
        tarifasAplicadasHabitaciones ??= [];
        servicios ??= [];

        if (tarifasAplicadasHabitaciones.Any(t => t < 0))
            throw new ValidationException("TarifaAplicada no puede ser negativa.");

        if (servicios.Any(s => s.Cantidad <= 0))
            throw new ValidationException("Cantidad de servicio debe ser mayor a 0.");

        if (servicios.Any(s => s.PrecioUnitarioAplicado < 0))
            throw new ValidationException("PrecioUnitarioAplicado no puede ser negativo.");

        var totalHabitaciones = tarifasAplicadasHabitaciones.Sum();
        var totalServicios = servicios.Sum(s => s.Cantidad * s.PrecioUnitarioAplicado);
        var totalReserva = totalHabitaciones + totalServicios;

        return new ReservaCostBreakdown(totalHabitaciones, totalServicios, totalReserva);
    }
}
