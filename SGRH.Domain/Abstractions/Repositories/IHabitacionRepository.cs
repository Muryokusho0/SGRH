using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IHabitacionRepository : IRepository<Habitacion, int>
{
    Task<Habitacion?> GetByIdWithHistorialAsync(
        int id, CancellationToken ct = default);

    Task<bool> ExistsByNumeroAsync(
        int numero, CancellationToken ct = default);

    // Sin filtro de categoría — lo usa ReservaDomainPolicy.
    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default);

    // Con filtro de categoría — lo usa ListarHabitacionesDisponiblesUseCase.
    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, int? categoriaHabitacionId,
        CancellationToken ct = default);

    Task<List<Habitacion>> BuscarAsync(
        string? estado, int? categoriaId, int? piso,
        CancellationToken ct = default);
}