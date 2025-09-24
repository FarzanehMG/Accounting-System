using Domain.Entities.FinancialPeriod;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.FinancialPeriod;

internal class FinancialPeriodEntityConfiguration : IEntityTypeConfiguration<FinancialPeriodEntity>
{
    public void Configure(EntityTypeBuilder<FinancialPeriodEntity> builder)
    {
        builder.ToTable("FinancialPeriods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FinancialPeriodTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.FinancialPeriodTitle);
        builder.HasIndex(x => new { x.StartDate, x.EndDate });

        // Navigation
        builder.HasMany(x => x.JournalVouchers)
            .WithOne(x => x.FinancialPeriod)
            .HasForeignKey(x => x.FinancialPeriodId);
    }
}