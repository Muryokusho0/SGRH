using SGRH.Web.Models.Habitaciones;

namespace SGRH.Web.Services.Habitaciones;

/// <summary>
/// Contrato del servicio de habitaciones para el portal del cliente.
/// Solo expone habitaciones disponibles; el historial de estado
/// interno nunca se incluye en los resultados.
/// </summary>
public interface IHabitacionService
{
    /// <summary>
    /// Devuelve las habitaciones disponibles para el rango de fechas indicado.
    /// El historial de estado de cada habitación NO se incluye: el cliente
    /// solo necesita número, piso, categoría y tarifa para elegir.
    /// </summary>
    /// <param name="entrada">Fecha de check-in deseada.</param>
    /// <param name="salida">Fecha de check-out deseada.</param>
    /// <param name="categoriaId">
    /// Filtro opcional por categoría. El cliente lo selecciona desde un dropdown,
    /// nunca ingresa el ID manualmente.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    Task<List<HabitacionDisponibleViewModel>> ListarDisponiblesAsync(
        DateTime entrada,
        DateTime salida,
        int? categoriaId = null,
        CancellationToken ct = default);
}