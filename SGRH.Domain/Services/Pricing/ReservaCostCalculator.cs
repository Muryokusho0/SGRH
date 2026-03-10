using SGRH.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Services.Pricing;

public sealed class ReservaCostCalculator : IReservaCostCalculator
{
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
