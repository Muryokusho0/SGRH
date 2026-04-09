using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Reservas;

// ─────────────────────────────────────────────────────────────────────────────
// LECTURA — devueltos por la API, sin validaciones de entrada
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Resumen de reserva para listados.</summary>
public sealed class ReservaViewModel
{
    internal int ReservaId { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaReserva { get; init; }
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public decimal CostoTotal { get; init; }
    public int TotalHabitaciones { get; init; }
    public int TotalServicios { get; init; }

    public int Noches =>
        Math.Max((int)(FechaSalida - FechaEntrada).TotalDays, 0);
}

/// <summary>Detalle completo de una reserva.</summary>
public sealed class DetalleReservaViewModel
{
    internal int ReservaId { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaEntrada { get; init; }
    public DateTime FechaSalida { get; init; }
    public decimal CostoTotal { get; init; }

    public List<HabitacionReservaViewModel> Habitaciones { get; init; } = [];
    public List<ServicioReservaViewModel> Servicios { get; init; } = [];

    public int Noches =>
        Math.Max((int)(FechaSalida - FechaEntrada).TotalDays, 0);

    /// <summary>Solo las reservas Pendiente son editables.</summary>
    public bool EsModificable => Estado == "Pendiente";
}

/// <summary>Habitación dentro de un detalle de reserva.</summary>
public sealed class HabitacionReservaViewModel
{
    internal int HabitacionId { get; init; }
    public int NumeroHabitacion { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public decimal TarifaAplicada { get; init; }
}

/// <summary>Servicio adicional dentro de un detalle de reserva.</summary>
public sealed class ServicioReservaViewModel
{
    internal int ServicioAdicionalId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}

/// <summary>Servicio seleccionado en el wizard (con cantidad mutable).</summary>
public sealed class ServicioSeleccionadoViewModel
{
    internal int ServicioAdicionalId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal PrecioUnitario { get; init; }
    public int Cantidad { get; set; } = 1;

    public decimal Subtotal => PrecioUnitario * Cantidad;
}

// ─────────────────────────────────────────────────────────────────────────────
// ESCRITURA — enviados a la API, con DataAnnotations para validación en cliente
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Datos para crear una nueva reserva (paso 1 del wizard).
/// Las validaciones de fecha se hacen con IValidatableObject porque
/// [Range] no soporta DateTime dinámico basado en DateTime.Today.
/// </summary>
public sealed class NuevaReservaFechasViewModel : IValidatableObject
{
    [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
    public DateTime? FechaEntrada { get; set; }

    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime? FechaSalida { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (FechaEntrada.HasValue && FechaEntrada.Value.Date <= DateTime.Today)
            yield return new ValidationResult(
                "La fecha de entrada debe ser a partir de mañana.",
                [nameof(FechaEntrada)]);

        if (FechaEntrada.HasValue && FechaSalida.HasValue
            && FechaSalida.Value <= FechaEntrada.Value)
            yield return new ValidationResult(
                "La fecha de salida debe ser posterior a la de entrada.",
                [nameof(FechaSalida)]);
    }
}

/// <summary>
/// Datos para cambiar las fechas de una reserva existente.
/// Mismas reglas que NuevaReservaFechasViewModel.
/// </summary>
public sealed class CambiarFechasViewModel : IValidatableObject
{
    [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
    public DateTime? NuevaEntrada { get; set; }

    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime? NuevaSalida { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (NuevaEntrada.HasValue && NuevaEntrada.Value.Date <= DateTime.Today)
            yield return new ValidationResult(
                "La fecha de entrada debe ser a partir de mañana.",
                [nameof(NuevaEntrada)]);

        if (NuevaEntrada.HasValue && NuevaSalida.HasValue
            && NuevaSalida.Value <= NuevaEntrada.Value)
            yield return new ValidationResult(
                "La fecha de salida debe ser posterior a la de entrada.",
                [nameof(NuevaSalida)]);
    }
}