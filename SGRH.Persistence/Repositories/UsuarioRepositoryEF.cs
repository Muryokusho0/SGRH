using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Seguridad;
using SGRH.Domain.Enums;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

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

    public Task<List<Usuario>> BuscarAsync(
        string? rol, bool? activo, CancellationToken ct = default)
    {
        var query = Db.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(rol) && Enum.TryParse<RolUsuario>(rol, out var rolEnum))
            query = query.Where(u => u.Rol == rolEnum);

        if (activo.HasValue)
            query = query.Where(u => u.Activo == activo.Value);

        return query.OrderBy(u => u.Username).ToListAsync(ct);
    }
}