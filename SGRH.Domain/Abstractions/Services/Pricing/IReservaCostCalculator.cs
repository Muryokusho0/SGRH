using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services.Pricing;

/// <summary>
/// Define el contrato del servicio que calcula el desglose de costos de una reserva
/// a partir de los snapshots de tarifas y precios ya congelados.
/// No realiza consultas a la base de datos; opera solo con los valores en memoria.
/// </summary>
public interface IReservaCostCalculator
{
    /// <summary>
    /// Calcula el desglose de costos de una reserva a partir de las tarifas y servicios indicados.
    /// </summary>
    /// <param name="tarifasAplicadasHabitaciones">
    /// Lista de tarifas por noche (snapshots congelados) de cada habitación en la reserva.
    /// </param>
    /// <param name="servicios">
    /// Lista de tuplas con la cantidad y el precio unitario (snapshot) de cada servicio adicional.
    /// </param>
    /// <returns>
    /// Un <see cref="ReservaCostBreakdown"/> con el total de habitaciones, total de servicios
    /// y el costo total de la reserva.
    /// </returns>
    ReservaCostBreakdown Calculate(
        IReadOnlyList<decimal> tarifasAplicadasHabitaciones,
        IReadOnlyList<(int Cantidad, decimal PrecioUnitarioAplicado)> servicios);
}
