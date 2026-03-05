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

    public Task<ServicioAdicional?> GetWithPreciosAsync(int servicioId, CancellationToken ct = default)
        => Db.ServiciosAdicionales
            .Include(s => s.Precios)
            .Include(s => s.Temporadas)
            .FirstOrDefaultAsync(s => s.ServicioAdicionalId == servicioId, ct);
}