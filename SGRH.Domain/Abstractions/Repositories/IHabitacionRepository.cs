using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IHabitacionRepository : IRepository<Habitacion, int>
{
    // Carga la habitación con su historial de estados.
    // Necesario para CambiarEstado(), que opera sobre el historial.
    Task<Habitacion?> GetByIdWithHistorialAsync(int id, CancellationToken ct = default);

    // Verifica unicidad del número de habitación.
    Task<bool> ExistsByNumeroAsync(int numero, CancellationToken ct = default);

    // Habitaciones disponibles para un rango de fechas.
    // Lo usa el módulo de reservas para mostrar opciones al recepcionista.
    Task<List<Habitacion>> GetDisponiblesAsync(
        DateTime entrada, DateTime salida, CancellationToken ct = default);
}
