using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IHabitacionRepository : IRepository<Habitacion, int>
{
    Task<Habitacion?> GetByIdWithHistorialAsync(
        int id, CancellationToken ct = default);

    Task<bool> ExistsByNumeroAsync(
        int numero, CancellationToken ct = default);

    // Resuelve el HabitacionId a partir del número visible al cliente.
    Task<Habitacion?> GetByNumeroAsync(
        int numero, CancellationToken ct = default);

    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default);

    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaHabitacionId,
        CancellationToken ct = default);

    Task<List<Habitacion>> BuscarAsync(
        string? estado, int? categoriaId, int? piso,
        CancellationToken ct = default);
}