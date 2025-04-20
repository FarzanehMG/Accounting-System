using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<GroupAccount> GroupAccounts { get; init; } = default!;
	public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts { get; init; } = default!;
	public DbSet<SubLedgerAccount> SubLedgerAccounts { get; init; } = default!;
	public DbSet<DetailedAccount> DetailedAccounts { get; init; } = default!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<GroupAccount>(entity =>
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

			entity.HasMany<GeneralLedgerAccount>()
				.WithOne(x => x.GroupAccount)
				.HasForeignKey(x => x.GroupAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<GeneralLedgerAccount>(entity =>
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

			entity.HasMany<SubLedgerAccount>()
				.WithOne(x => x.GeneralLedgerAccount)
				.HasForeignKey(x => x.GeneralLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<SubLedgerAccount>(entity =>
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

			entity.HasMany<DetailedAccount>()
				.WithOne(x => x.SubLedgerAccount)
				.HasForeignKey(x => x.SubLedgerAccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<DetailedAccount>(entity =>
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
		});
	}
}