using System;

namespace Desktop.Models.Reportes;

public sealed class OcupacionActiva
{
    public int ReservaId { get; init; }
    public int HabitacionId { get; init; }
    public string HabitacionCodigo { get; init; } = string.Empty;
    public string CategoriaNombre { get; init; } = string.Empty;
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public string EstadoReserva { get; init; } = string.Empty;
    public string ClienteNombre { get; init; } = string.Empty;
}

public sealed class ReservaCosto
{
    public int ReservaId { get; init; }
    public string ClienteNombre { get; init; } = string.Empty;
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public decimal TotalHabitaciones { get; init; }
    public decimal TotalServicios { get; init; }
    public decimal Total => TotalHabitaciones + TotalServicios;
}