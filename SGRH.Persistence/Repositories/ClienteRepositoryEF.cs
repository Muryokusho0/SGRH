using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Clientes;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class ClienteRepositoryEF : Repository<Cliente, int>, IClienteRepository
{
    public ClienteRepositoryEF(SGRHDbContext db, ILogger<ClienteRepositoryEF> logger) : base(db, logger) { }

    public Task<Cliente?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default)
        => Db.Clientes.FirstOrDefaultAsync(c => c.NationalId == nationalId, ct);

    public Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Db.Clientes.FirstOrDefaultAsync(c => c.Email == email, ct);

    public Task<List<Cliente>> BuscarAsync(
        string? nombre,
        string? email,
        string? nationalId,
        CancellationToken ct = default)
    {
        var query = Db.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(c =>
                c.NombreCliente.Contains(nombre) ||
                c.ApellidoCliente.Contains(nombre));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(c => c.Email.Contains(email));

        if (!string.IsNullOrWhiteSpace(nationalId))
            query = query.Where(c => c.NationalId == nationalId);

        return query
            .OrderBy(c => c.ApellidoCliente)
            .ThenBy(c => c.NombreCliente)
            .ToListAsync(ct);
    }
}