using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Servicios;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class ServicioAdicionalRepositoryEF
    : Repository<ServicioAdicional, int>, IServicioAdicionalRepository
{
    public ServicioAdicionalRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<ServicioAdicional?> GetByIdWithTemporadasAsync(
        int id, CancellationToken ct = default)
        => Db.ServiciosAdicionales
            .FirstOrDefaultAsync(s => s.ServicioAdicionalId == id, ct);
}