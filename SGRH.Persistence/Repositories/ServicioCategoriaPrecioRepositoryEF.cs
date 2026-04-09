using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

public sealed class ServicioCategoriaPrecioRepositoryEF
    : Repository<ServicioCategoriaPrecio, (int ServicioId, int CategoriaId)>,
      IServicioCategoriaPrecioRepository
{
    public ServicioCategoriaPrecioRepositoryEF(SGRHDbContext db, ILogger<ServicioCategoriaPrecioRepositoryEF> logger) : base(db, logger) { }

    public override Task<ServicioCategoriaPrecio?> GetByIdAsync(
        (int ServicioId, int CategoriaId) id,
        CancellationToken ct = default)
        => Db.ServicioCategoriaPrecios
            .FindAsync([id.ServicioId, id.CategoriaId], ct)
            .AsTask();

    public async Task<decimal?> GetPrecioAsync(
        int servicioId, int categoriaId, CancellationToken ct = default)
    {
        var result = await Db.ServicioCategoriaPrecios
            .AsNoTracking()
            .Where(s => s.ServicioAdicionalId == servicioId
                     && s.CategoriaHabitacionId == categoriaId)
            .Select(s => (decimal?)s.Precio)
            .FirstOrDefaultAsync(ct);

        return result;
    }
}