using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services.Pricing;

public interface IReservaCostCalculator
{
    // Calcula el desglose de costos de una reserva a partir de sus snapshots.
    // Recibe solo los valores ya congelados — no consulta nada a BD.
    ReservaCostBreakdown Calculate(
        IReadOnlyList<decimal> tarifasAplicadasHabitaciones,
        IReadOnlyList<(int Cantidad, decimal PrecioUnitarioAplicado)> servicios);
}
