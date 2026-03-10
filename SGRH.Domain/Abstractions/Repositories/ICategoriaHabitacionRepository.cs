using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Habitaciones;

namespace SGRH.Domain.Abstractions.Repositories;

public interface ICategoriaHabitacionRepository : IRepository<CategoriaHabitacion, int>
{
    // Verifica unicidad de nombre antes de crear/actualizar.
    Task<bool> ExistsByNombreAsync(string nombre, CancellationToken ct = default);
}