using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Clientes;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class ClienteRepositoryEF : Repository<Cliente, int>, IClienteRepository
{
    public ClienteRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Cliente?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default)
        => Db.Clientes.FirstOrDefaultAsync(c => c.NationalId == nationalId, ct);

    public Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Db.Clientes.FirstOrDefaultAsync(c => c.Email == email, ct);
}
