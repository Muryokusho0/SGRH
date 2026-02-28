using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IHabitacionRepository : IRepository<Habitacion, int>
{
    Task<Habitacion?> GetWithHistorialAsync(int habitacionId, CancellationToken ct = default);

    Task<List<Habitacion>> GetByCategoriaAsync(int categoriaId, CancellationToken ct = default);
}
