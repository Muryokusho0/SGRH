using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;
using SGRH.Domain.Abstractions.Repositories;

namespace SGRH.Persistence.Repositories.Base;

/// <summary>
/// Implementación base de IRepository usando Entity Framework Core.
/// Todas las implementaciones concretas heredan de esta clase.
/// </summary>
public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly SGRHDbContext Db;
    protected readonly DbSet<TEntity> Set;

    protected Repository(SGRHDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(TEntity entity)
        => Set.Update(entity);

    public virtual void Delete(TEntity entity)
        => Set.Remove(entity);

    /// <summary>
    /// Acceso al IQueryable para consultas personalizadas en subclases.
    /// No forma parte del contrato público IRepository.
    /// </summary>
    protected IQueryable<TEntity> Query()
        => Set.AsQueryable();
}