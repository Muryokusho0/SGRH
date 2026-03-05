using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Services.Pricing;

public interface IReservaCostCalculator
{
    /// <summary>
    /// Calcula:
    /// - TotalHabitaciones = SUM(TarifaAplicada)
    /// - TotalServicios    = SUM(Cantidad * PrecioUnitarioAplicado)
    /// - TotalReserva      = TotalHabitaciones + TotalServicios
    /// </summary>
    ReservaCostBreakdown Calculate(
        IReadOnlyList<decimal> tarifasAplicadasHabitaciones,
        IReadOnlyList<(int Cantidad, decimal PrecioUnitarioAplicado)> servicios);
}
