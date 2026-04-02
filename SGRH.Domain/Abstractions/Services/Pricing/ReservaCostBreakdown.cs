using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Services.Pricing;

/// <summary>
/// Representa el desglose detallado de los costos calculados de una reserva,
/// separando el total de habitaciones del total de servicios adicionales.
/// </summary>
/// <param name="TotalHabitaciones">
/// Suma de todas las tarifas por noche de las habitaciones incluidas en la reserva.
/// </param>
/// <param name="TotalServicios">
/// Suma de los subtotales (cantidad × precio unitario) de todos los servicios adicionales.
/// </param>
/// <param name="TotalReserva">
/// Costo total de la reserva: <c>TotalHabitaciones + TotalServicios</c>.
/// </param>
public sealed record ReservaCostBreakdown(
    decimal TotalHabitaciones,
    decimal TotalServicios,
    decimal TotalReserva);
