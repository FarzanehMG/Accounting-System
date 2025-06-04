using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<GroupAccountEntity> GroupAccounts { get; init; } = default!;
	public DbSet<GeneralLedgerAccountEntity> GeneralLedgerAccounts { get; init; } = default!;
	public DbSet<SubLedgerAccountEntity> SubLedgerAccounts { get; init; } = default!;
	public DbSet<DetailedAccountEntity> DetailedAccounts { get; init; } = default!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<GroupAccountEntity>(entity =>
		{
			entity.HasKey(x => x.Id);

			entity.Property(x => x.AccountTitle)
				.HasMaxLength(150)
				.IsRequired();

			entity.Property(x => x.AccountDescription)
				.HasMaxLength(300);

			entity.Property(x => x.AccountCode)
				.HasMaxLength(150)
				.IsRequired();

			entity.HasIndex(x => x.IsActive);

			entity.HasMany<GeneralLedgerAccountEntity>()
				.WithOne(x => x.GroupAccount)
				.HasForeignKey(x => x.GroupAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<GeneralLedgerAccountEntity>(entity =>
		{
			entity.HasKey(x => x.Id);

			entity.Property(x => x.AccountTitle)
				.HasMaxLength(150)
				.IsRequired();

			entity.Property(x => x.AccountDescription)
				.HasMaxLength(300);

			entity.Property(x => x.AccountCode)
				.HasMaxLength(150)
				.IsRequired();

			entity.HasIndex(x => x.IsActive);

			entity.HasMany<SubLedgerAccountEntity>()
				.WithOne(x => x.GeneralLedgerAccount)
				.HasForeignKey(x => x.GeneralLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(x => x.GroupAccount)
				.WithMany(x => x.GeneralLedgerAccounts)
				.HasForeignKey(x => x.GroupAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<SubLedgerAccountEntity>(entity =>
		{
			entity.HasKey(x => x.Id);

			entity.Property(x => x.AccountTitle)
				.HasMaxLength(150)
				.IsRequired();

			entity.Property(x => x.AccountDescription)
				.HasMaxLength(300);

			entity.Property(x => x.AccountCode)
				.HasMaxLength(150)
				.IsRequired();

			entity.HasIndex(x => x.IsActive);

			entity.HasMany<DetailedAccountEntity>()
				.WithOne(x => x.SubLedgerAccount)
				.HasForeignKey(x => x.SubLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(x => x.GeneralLedgerAccount)
				.WithMany(x => x.SubLedgerAccounts)
				.HasForeignKey(x => x.GeneralLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<DetailedAccountEntity>(entity =>
		{
			entity.HasKey(x => x.Id);

			entity.Property(x => x.AccountTitle)
				.HasMaxLength(150)
				.IsRequired();

			entity.Property(x => x.AccountDescription)
				.HasMaxLength(300);

			entity.Property(x => x.AccountCode)
				.HasMaxLength(150)
				.IsRequired();

			entity.HasIndex(x => x.IsActive);

			entity.HasOne(x => x.SubLedgerAccount)
				.WithMany(x => x.DetailedAccounts)
				.HasForeignKey(x => x.SubLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}