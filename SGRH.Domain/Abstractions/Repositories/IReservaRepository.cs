using SGRH.Domain.Entities.Reservas;

namespace SGRH.Domain.Abstractions.Repositories;

/// <summary>
/// Repositorio especializado para reservas del hotel.
/// Provee consultas con carga de detalles, verificación de disponibilidad y búsqueda filtrada.
/// </summary>
public interface IReservaRepository : IRepository<Reserva, int>
{
    /// <summary>
    /// Obtiene una reserva por su id, incluyendo sus habitaciones y servicios adicionales (con tracking de EF).
    /// </summary>
    /// <param name="reservaId">Id de la reserva a obtener.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La reserva con sus detalles cargados, o <c>null</c> si no existe.</returns>
    Task<Reserva?> GetByIdWithDetallesAsync(
        int reservaId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una reserva por su id con sus detalles, sin tracking de EF Core (solo lectura).
    /// </summary>
    /// <param name="reservaId">Id de la reserva a obtener.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La reserva con sus detalles (sin tracking), o <c>null</c> si no existe.</returns>
    Task<Reserva?> GetByIdWithDetallesAsNoTrackingAsync(
        int reservaId, CancellationToken ct = default);

    /// <summary>
    /// Actualiza directamente las fechas de entrada y salida de una reserva en la base de datos.
    /// </summary>
    /// <remarks>
    /// Ejecuta el UPDATE directamente para evitar conflictos con triggers de la BD
    /// que se activan al modificar las fechas mediante EF Core.
    /// </remarks>
    /// <param name="reservaId">Id de la reserva a actualizar.</param>
    /// <param name="nuevaEntrada">Nueva fecha de check-in.</param>
    /// <param name="nuevaSalida">Nueva fecha de check-out.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task ActualizarFechasAsync(
        int reservaId, DateTime nuevaEntrada, DateTime nuevaSalida,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las reservas asociadas a un cliente específico.
    /// </summary>
    /// <param name="clienteId">Id del cliente.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de reservas del cliente.</returns>
    Task<List<Reserva>> GetByClienteAsync(
        int clienteId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si una habitación tiene alguna reserva activa (Pendiente o Confirmada)
    /// que se solape con el rango de fechas indicado.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación a verificar.</param>
    /// <param name="entrada">Fecha de inicio del rango.</param>
    /// <param name="salida">Fecha de fin del rango.</param>
    /// <param name="excluirReservaId">Id de reserva a excluir de la verificación (opcional, para ediciones).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns><c>true</c> si existe un conflicto de fechas; de lo contrario, <c>false</c>.</returns>
    Task<bool> HabitacionTieneReservaActivaAsync(
        int habitacionId, DateTime entrada, DateTime salida,
        int? excluirReservaId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene los rangos de fechas en que una habitación está ocupada por reservas activas.
    /// Utilizado para mostrar al cliente cuándo una habitación no está disponible.
    /// </summary>
    /// <param name="habitacionId">Id de la habitación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de rangos de fechas con el estado de reserva correspondiente.</returns>
    Task<List<RangoOcupadoDto>> GetRangosOcupadosPorHabitacionAsync(
        int habitacionId, CancellationToken ct = default);

    /// <summary>
    /// Busca reservas con múltiples filtros opcionales combinados.
    /// </summary>
    /// <param name="clienteId">Filtrar por cliente (opcional).</param>
    /// <param name="estado">Filtrar por estado de reserva (opcional).</param>
    /// <param name="fechaDesde">Fecha de entrada mínima (opcional).</param>
    /// <param name="fechaHasta">Fecha de salida máxima (opcional).</param>
    /// <param name="reservadaDesde">Fecha de creación mínima de la reserva (opcional).</param>
    /// <param name="reservadaHasta">Fecha de creación máxima de la reserva (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de reservas que cumplen todos los filtros aplicados.</returns>
    Task<List<Reserva>> BuscarAsync(
        int? clienteId, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta,
        DateTime? reservadaDesde, DateTime? reservadaHasta,
        CancellationToken ct = default);
}

/// <summary>
/// DTO ligero que representa un rango de fechas en que una habitación está ocupada,
/// junto con el estado de la reserva correspondiente.
/// </summary>
/// <param name="FechaEntrada">Fecha de inicio del período de ocupación (check-in).</param>
/// <param name="FechaSalida">Fecha de fin del período de ocupación (check-out).</param>
/// <param name="Estado">Estado de la reserva que genera la ocupación (por ejemplo: "Confirmada").</param>
public sealed record RangoOcupadoDto(
    DateTime FechaEntrada,
    DateTime FechaSalida,
    string Estado);