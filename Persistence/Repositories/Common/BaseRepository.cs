using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories.Common;

internal abstract class BaseRepository<TEntity>(AccountingDbContext db) where TEntity : class, IEntity
{
    private protected readonly DbSet<TEntity> _entities = db.Set<TEntity>();

    private protected DbSet<TEntity> Entities => _entities;
    private protected IQueryable<TEntity> Table => _entities;

    private protected IQueryable<TEntity> TableNoTracking => _entities.AsNoTracking();

    protected virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(entity, cancellationToken);
    }

    protected virtual void Remove(TEntity entity)
    {
        _entities.Remove(entity);
    }
}