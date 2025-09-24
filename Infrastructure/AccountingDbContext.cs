using Domain.Common;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.RegisterEntities<IEntity>(typeof(IEntity).Assembly);
        modelBuilder.ApplyRestrictDeleteBehaviour();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyEntityChangeDates();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityChangeDates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        ApplyEntityChangeDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        ApplyEntityChangeDates();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyEntityChangeDates()
    {
        var entities = base.ChangeTracker.Entries<IEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified);

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
                entity.Entity.CreatedDate = DateTime.UtcNow;

            else if (entity.State == EntityState.Modified)
                entity.Entity.ModifiedDate = DateTime.UtcNow;
        }
    }
}