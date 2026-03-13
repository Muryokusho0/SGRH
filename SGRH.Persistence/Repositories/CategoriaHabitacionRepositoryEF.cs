using Microsoft.EntityFrameworkCore;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Entities.Habitaciones;
using SGRH.Persistence.Context;
using SGRH.Persistence.Repositories.Base;

namespace SGRH.Persistence.Repositories;

public sealed class CategoriaHabitacionRepositoryEF
    : Repository<CategoriaHabitacion, int>, ICategoriaHabitacionRepository
{
    public CategoriaHabitacionRepositoryEF(SGRHDbContext db) : base(db) { }

    public Task<bool> ExistsByNombreAsync(string nombre, CancellationToken ct = default)
        => Db.CategoriasHabitacion
            .AnyAsync(c => c.NombreCategoria == nombre, ct);

    public Task<List<CategoriaHabitacion>> BuscarAsync(
        string? nombre,
        int? capacidadMinima,
        int? capacidadMaxima,
        CancellationToken ct = default)
    {
        var query = Db.CategoriasHabitacion.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(c => c.NombreCategoria.Contains(nombre));

        if (capacidadMinima.HasValue)
            query = query.Where(c => c.Capacidad >= capacidadMinima.Value);

        if (capacidadMaxima.HasValue)
            query = query.Where(c => c.Capacidad <= capacidadMaxima.Value);

        return query.OrderBy(c => c.NombreCategoria).ToListAsync(ct);
    }
}