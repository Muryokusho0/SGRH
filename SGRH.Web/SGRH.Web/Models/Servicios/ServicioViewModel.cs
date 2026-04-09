namespace SGRH.Web.Models.Servicios;

/// <summary>
/// Servicio adicional disponible para agregar a una reserva.
/// El cliente lo selecciona desde una lista o tarjeta;
/// nunca ingresa el ID ni el nombre de forma manual.
/// </summary>
public sealed class ServicioViewModel
{
    /// <summary>ID interno — solo para la llamada HTTP de agregar servicio.</summary>
    internal int ServicioAdicionalId { get; init; }

    public string Nombre { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal PrecioUnitario { get; init; }
}