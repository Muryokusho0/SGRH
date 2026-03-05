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
    : Repository<ServicioCategoriaPrecio, (int ServicioId, int CategoriaId)>, IServicioCategoriaPrecioRepository
{
    public ServicioCategoriaPrecioRepositoryEF(SGRHDbContext db) : base(db) { }

    public override Task<ServicioCategoriaPrecio?> GetByIdAsync((int ServicioId, int CategoriaId) id, CancellationToken ct = default)
        => Db.ServicioCategoriaPrecios.FindAsync([id.ServicioId, id.CategoriaId], ct).AsTask();
}
