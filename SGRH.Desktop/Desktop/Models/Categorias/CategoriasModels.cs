namespace Desktop.Models.Categorias;

public sealed class CategoriaResumen
{
    public int CategoriaHabitacionId { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public int Capacidad { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public decimal PrecioBase { get; init; }
}