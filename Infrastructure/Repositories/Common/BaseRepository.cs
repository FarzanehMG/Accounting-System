using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Common;

internal abstract class BaseRepository<TEntity>(AccountingDbContext db) where TEntity : class, IEntity
{

    private readonly DbSet<TEntity> _entities = db.Set<TEntity>();

    private protected IQueryable<TEntity> Table => _entities;

    private protected IQueryable<TEntity> TableNoTracking => _entities.AsNoTracking();

    protected virtual async ValueTask AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(entity, cancellationToken);
    }
}