using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para habitaciones del hotel.
/// Provee consultas por número, disponibilidad en fechas y búsqueda filtrada.
/// </summary>
public interface IHabitacionRepository : IRepository<Habitacion, int>
{
    /// <summary>
    /// Obtiene una habitación por su id, incluyendo el historial de estados cargado.
    /// </summary>
    /// <param name="id">Id de la habitación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La habitación con su historial, o <c>null</c> si no existe.</returns>
    Task<Habitacion?> GetByIdWithHistorialAsync(
        int id, CancellationToken ct = default);

    /// <summary>
    /// Verifica si ya existe una habitación con el número indicado.
    /// </summary>
    /// <param name="numero">Número de habitación a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si ya existe una habitación con ese número; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsByNumeroAsync(
        int numero, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una habitación a partir de su número visible (el que se muestra al cliente).
    /// </summary>
    /// <param name="numero">Número visible de la habitación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La habitación encontrada, o <c>null</c> si no existe.</returns>
    Task<Habitacion?> GetByNumeroAsync(
        int numero, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las habitaciones disponibles para reservar en el rango de fechas indicado.
    /// </summary>
    /// <param name="entrada">Fecha de check-in deseada.</param>
    /// <param name="salida">Fecha de check-out deseada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de habitaciones disponibles en el rango de fechas.</returns>
    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default);

    /// <summary>
    /// Obtiene las habitaciones disponibles en el rango de fechas indicado,
    /// opcionalmente filtradas por categoría de habitación.
    /// </summary>
    /// <param name="entrada">Fecha de check-in deseada.</param>
    /// <param name="salida">Fecha de check-out deseada.</param>
    /// <param name="categoriaHabitacionId">Id de la categoría para filtrar (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de habitaciones disponibles que cumplen los filtros.</returns>
    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaHabitacionId,
        CancellationToken ct = default);

    /// <summary>
    /// Busca habitaciones con filtros opcionales de estado, categoría y piso.
    /// </summary>
    /// <param name="estado">Filtrar por estado operativo (por ejemplo: "Disponible", "Ocupada") (opcional).</param>
    /// <param name="categoriaId">Filtrar por id de categoría de habitación (opcional).</param>
    /// <param name="piso">Filtrar por número de piso (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de habitaciones que cumplen los filtros indicados.</returns>
    Task<List<Habitacion>> BuscarAsync(
        string? estado, int? categoriaId, int? piso,
        CancellationToken ct = default);
}