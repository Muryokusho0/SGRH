using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;

namespace SGRH.Persistence.Repositories.Base;

public interface IRepository<TEntity, TKey>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    IQueryable<TEntity> Query();
}

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly SGRHDbContext Db;
    protected readonly DbSet<TEntity> Set;

    public Repository(SGRHDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    public virtual Task AddAsync(TEntity entity, CancellationToken ct = default)
        => Set.AddAsync(entity, ct).AsTask();

    public virtual void Update(TEntity entity) => Set.Update(entity);
    public virtual void Remove(TEntity entity) => Set.Remove(entity);
    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();
}