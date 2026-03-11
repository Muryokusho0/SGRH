using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Auditoria;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SGRH.Persistence.Repositories;

public sealed class AuditoriaRepositoryEF
    : Repository<AuditoriaEvento, long>, IAuditoriaRepository
{
    public AuditoriaRepositoryEF(SGRHDbContext db) : base(db) { }

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
}
