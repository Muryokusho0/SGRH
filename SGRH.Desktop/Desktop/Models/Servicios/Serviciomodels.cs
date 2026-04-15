namespace Desktop.Models.Servicios;

public sealed class ServicioResumen
{
    public int ServicioAdicionalId { get; init; }
    public string NombreServicio { get; init; } = string.Empty;
    public string TipoServicio { get; init; } = string.Empty;
    public bool AplicaTodasTemporadas { get; init; }
}