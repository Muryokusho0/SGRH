namespace SGRH.Web.Models.Temporadas;

/// <summary>
/// Temporada visible para el cliente: solo nombre y rango de fechas.
/// El ID interno no se expone.
/// </summary>
public sealed class TemporadaViewModel
{
    public string Nombre { get; init; } = string.Empty;
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }

    /// <summary>Indica si la temporada está activa hoy.</summary>
    public bool EsActual
    {
        get
        {
            var hoy = DateTime.Today;
            return FechaInicio.HasValue && FechaFin.HasValue
                && hoy >= FechaInicio.Value.Date
                && hoy <= FechaFin.Value.Date;
        }
    }

    /// <summary>Rango de fechas formateado para la UI.</summary>
    public string RangoTexto
    {
        get
        {
            if (FechaInicio.HasValue && FechaFin.HasValue)
                return $"{FechaInicio.Value:dd MMM yyyy} — {FechaFin.Value:dd MMM yyyy}";
            if (FechaInicio.HasValue)
                return $"Desde {FechaInicio.Value:dd MMM yyyy}";
            return "Fechas por definir";
        }
    }
}