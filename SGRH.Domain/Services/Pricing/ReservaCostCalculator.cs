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
            throw new ValidationException(["TarifaAplicada no puede ser negativa."]);

        if (servicios.Any(s => s.Cantidad <= 0))
            throw new ValidationException(["Cantidad de servicio debe ser > 0."]);

        if (servicios.Any(s => s.PrecioUnitarioAplicado < 0))
            throw new ValidationException(["PrecioUnitarioAplicado no puede ser negativo."]);

        decimal totalHabitaciones = tarifasAplicadasHabitaciones.Sum();
        decimal totalServicios = servicios.Sum(s => s.Cantidad * s.PrecioUnitarioAplicado);
        decimal totalReserva = totalHabitaciones + totalServicios;

        // Regla de consistencia: no forzamos mínimo, pero validamos integridad básica.
        if (totalReserva < 0)
            throw new BusinessRuleViolationException("El total de la reserva no puede ser negativo.");

        return new ReservaCostBreakdown(totalHabitaciones, totalServicios, totalReserva);
    }
}
