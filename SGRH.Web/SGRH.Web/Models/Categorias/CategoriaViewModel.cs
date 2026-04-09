namespace SGRH.Web.Models.Categorias;

/// <summary>
/// Categoría de habitación usada como filtro en la búsqueda de disponibilidad.
/// El cliente la selecciona desde un dropdown;
/// nunca ingresa el ID ni el nombre de forma manual.
/// </summary>
public sealed class CategoriaViewModel
{
    /// <summary>ID interno — solo para el filtro en la llamada HTTP de disponibilidad.</summary>
    internal int CategoriaHabitacionId { get; init; }

    public string Nombre { get; init; } = string.Empty;
    public int Capacidad { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public decimal PrecioBase { get; init; }

    /// <summary>
    /// Etiqueta amigable para el dropdown
    /// (ej. "Suite — desde RD$ 5,000.00").
    /// </summary>
    public string OpcionDropdown =>
        $"{Nombre} — desde RD$ {PrecioBase:N2}";
}