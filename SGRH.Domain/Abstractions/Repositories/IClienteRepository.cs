using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Domain.Entities.Clientes;

namespace SGRH.Domain.Abstractions.Repositories;

public interface IClienteRepository : IRepository<Cliente, int>
{
    Task<Cliente?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default);

    Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<List<Cliente>> BuscarAsync(
        string? nombre,
        string? email,
        string? nationalId,
        CancellationToken ct = default);
}
