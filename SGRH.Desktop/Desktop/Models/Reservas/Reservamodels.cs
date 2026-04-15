using System;
using System.Collections.Generic;

namespace Desktop.Models.Reservas;

public sealed class ReservaResumen
{
    public int ReservaId { get; init; }
    public string ClienteNombre { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaReserva { get; init; }
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public decimal CostoTotal { get; init; }
    public int Noches => Math.Max((int)(FechaSalida - FechaEntrada).TotalDays, 0);
}

public sealed class ReservaDetalle
{
    public int ReservaId { get; init; }
    public int ClienteId { get; init; }
    public string ClienteNombre { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public decimal CostoTotal { get; init; }
    public List<HabitacionReserva> Habitaciones { get; init; } = [];
    public List<ServicioReserva> Servicios { get; init; } = [];
    public bool EsModificable => Estado == "Pendiente";
    public int Noches => Math.Max((int)(FechaSalida - FechaEntrada).TotalDays, 0);
}

public sealed class HabitacionReserva
{
    public int HabitacionId { get; init; }
    public int NumeroHabitacion { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public decimal TarifaAplicada { get; init; }
}

public sealed class ServicioReserva
{
    public int ServicioAdicionalId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}