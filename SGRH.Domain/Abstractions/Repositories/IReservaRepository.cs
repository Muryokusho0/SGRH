using SGRH.Domain.Entities.Reservas;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IReservaRepository : IRepository<Reserva, int>
{
    Task<Reserva?> GetByIdWithDetallesAsync(
        int reservaId, CancellationToken ct = default);

    Task<Reserva?> GetByIdWithDetallesAsNoTrackingAsync(
        int reservaId, CancellationToken ct = default);

    Task ActualizarFechasAsync(
        int reservaId, DateTime nuevaEntrada, DateTime nuevaSalida,
        CancellationToken ct = default);

    Task<List<Reserva>> GetByClienteAsync(
        int clienteId, CancellationToken ct = default);

    Task<bool> HabitacionTieneReservaActivaAsync(
        int habitacionId, DateTime entrada, DateTime salida,
        int? excluirReservaId, CancellationToken ct = default);

    // Devuelve los rangos de fechas en que cada habitación está ocupada.
    // Se usa para mostrar al cliente cuándo una habitación no está disponible.
    Task<List<RangoOcupadoDto>> GetRangosOcupadosPorHabitacionAsync(
        int habitacionId, CancellationToken ct = default);

    Task<List<Reserva>> BuscarAsync(
        int? clienteId, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta,
        DateTime? reservadaDesde, DateTime? reservadaHasta,
        CancellationToken ct = default);
}

// DTO ligero — no necesita una capa Application completa
public sealed record RangoOcupadoDto(
    DateTime FechaEntrada,
    DateTime FechaSalida,
    string Estado);