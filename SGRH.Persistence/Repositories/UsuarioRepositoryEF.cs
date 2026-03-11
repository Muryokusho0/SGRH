using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class UsuarioRepositoryEF
    : Repository<Usuario, int>, IUsuarioRepository
{
    public UsuarioRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<Usuario?> GetByUsernameAsync(
        string username, CancellationToken ct = default)
        => Db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByUsernameAsync(
        string username, CancellationToken ct = default)
        => Db.Usuarios
            .AnyAsync(u => u.Username == username, ct);
}