using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Habitaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IHabitacionHistorialRepository : IRepository<HabitacionHistorial, int>
{
    Task<HabitacionHistorial?> GetVigenteAsync(
        int habitacionId, CancellationToken ct = default);
}