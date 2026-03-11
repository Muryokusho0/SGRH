using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Persistence.Repositories;

public sealed class CategoriaHabitacionRepositoryEF
    : Repository<CategoriaHabitacion, int>, ICategoriaHabitacionRepository
{
    public CategoriaHabitacionRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<bool> ExistsByNombreAsync(string nombre, CancellationToken ct = default)
        => Db.CategoriasHabitacion
            .AnyAsync(c => c.NombreCategoria == nombre, ct);
}