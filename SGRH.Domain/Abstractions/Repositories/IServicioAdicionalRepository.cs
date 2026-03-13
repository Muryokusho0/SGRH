using SGRH.Domain.Entities;
using SGRH.Domain.Entities.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IServicioAdicionalRepository : IRepository<ServicioAdicional, int>
{
    Task<bool> ExistsByNombreAsync(string nombreServicio, CancellationToken ct = default);

    Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default);

    Task<List<ServicioAdicional>> BuscarAsync(
        string? nombre, CancellationToken ct = default);
}
