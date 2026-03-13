using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class AuditoriaRepositoryEF
    : Repository<AuditoriaEvento, long>, IAuditoriaRepository
{
    public AuditoriaRepositoryEF(SGRHDbContext db) : base(db) { }

    public override Task<AuditoriaEvento?> GetByIdAsync(long id, CancellationToken ct = default)
        => Db.AuditoriaEventos.FirstOrDefaultAsync(a => a.AuditoriaEventoId == id, ct);

    public Task<List<AuditoriaEvento>> GetByUsuarioAsync(
        int usuarioId, CancellationToken ct = default)
        => Db.AuditoriaEventos
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.FechaUtc)
            .ToListAsync(ct);

    public Task<List<AuditoriaEvento>> GetByModuloAsync(
        string modulo, CancellationToken ct = default)
        => Db.AuditoriaEventos
            .AsNoTracking()
            .Where(a => a.Modulo == modulo)
            .OrderByDescending(a => a.FechaUtc)
            .ToListAsync(ct);

    public Task<List<AuditoriaEvento>> GetByRangoFechaAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
        => Db.AuditoriaEventos
            .AsNoTracking()
            .Where(a => a.FechaUtc >= desde && a.FechaUtc <= hasta)
            .OrderByDescending(a => a.FechaUtc)
            .ToListAsync(ct);

    public Task<List<AuditoriaEvento>> BuscarAsync(
        string? modulo,
        string? accion,
        string? entidad,
        int? usuarioId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken ct = default)
    {
        var query = Db.AuditoriaEventos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(a => a.Modulo == modulo);

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(a => a.Entidad == entidad);

        if (usuarioId.HasValue)
            query = query.Where(a => a.UsuarioId == usuarioId.Value);

        if (fechaDesde.HasValue)
            query = query.Where(a => a.FechaUtc >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(a => a.FechaUtc <= fechaHasta.Value);

        return query.OrderByDescending(a => a.FechaUtc).ToListAsync(ct);
    }
}