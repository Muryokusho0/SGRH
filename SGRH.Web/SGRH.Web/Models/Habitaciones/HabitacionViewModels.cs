namespace SGRH.Web.Models.Habitaciones;

/// <summary>
/// Habitación disponible que el cliente puede seleccionar para su reserva.
/// El historial de estado interno NO se expone: el cliente solo necesita
/// número, piso, categoría y tarifa para tomar su decisión.
/// El ID es <c>internal</c> para que el servicio HTTP lo use sin
/// que aparezca en ningún componente Razor.
/// </summary>
public sealed class HabitacionDisponibleViewModel
{
    /// <summary>ID interno — solo para la llamada HTTP de agregar habitación.</summary>
    internal int HabitacionId { get; init; }

    public int NumeroHabitacion { get; init; }
    public int Piso { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public decimal TarifaPorNoche { get; init; }

    // ── Propiedades derivadas para la UI ──────────────────────────────────

    /// <summary>Etiqueta amigable de número para mostrar en tarjeta (ej. "Hab. 101").</summary>
    public string NumeroTexto => $"Hab. {NumeroHabitacion}";

    /// <summary>Etiqueta amigable de piso para mostrar en tarjeta (ej. "Piso 1").</summary>
    public string PisoTexto => $"Piso {Piso}";
}