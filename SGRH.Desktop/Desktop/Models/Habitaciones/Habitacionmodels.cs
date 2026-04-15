namespace Desktop.Models.Habitaciones;

public sealed class HabitacionResumen
{
    public int HabitacionId { get; init; }
    public int NumeroHabitacion { get; init; }
    public int Piso { get; init; }
    public int CategoriaHabitacionId { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string EstadoActual { get; init; } = string.Empty;
    public string NumeroTexto => $"Hab. {NumeroHabitacion:000}";
    public string PisoTexto => $"Piso {Piso}";
}

public sealed class HabitacionDisponible
{
    public int HabitacionId { get; init; }
    public int NumeroHabitacion { get; init; }
    public int Piso { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public decimal TarifaPorNoche { get; init; }
    public string NumeroTexto => $"Hab. {NumeroHabitacion:000}";
    public string PisoTexto => $"Piso {Piso}";
}