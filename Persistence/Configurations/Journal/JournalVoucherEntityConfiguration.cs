using Domain.Entities.Journal;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configurations.Journal;

internal class JournalVoucherEntityConfiguration : IEntityTypeConfiguration<JournalVoucherEntity>
{
    public void Configure(EntityTypeBuilder<JournalVoucherEntity> builder)
    {
        builder.ToTable("JournalVouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JournalVoucherTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.JournalVoucherNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TransactionTime)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2048);

        builder.Property(x => x.JournalVoucherState)
            .IsRequired()
            .HasConversion<EnumToStringConverter<JournalVoucherState>>();

        builder.Property(x => x.JournalVoucherState)
            .HasMaxLength(20);

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.FinancialPeriodId)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.FinancialPeriodId);
        builder.HasIndex(x => new { x.JournalVoucherNumber , x.Id}).IsUnique();
        builder.HasIndex(x => x.TransactionTime);
        builder.HasIndex(x => x.JournalVoucherState);
        builder.HasIndex(x => x.ReferenceNumber);

        // Navigation
        builder.HasOne(x => x.FinancialPeriod)
            .WithMany(x => x.JournalVouchers)
            .HasForeignKey(x => x.FinancialPeriodId);

        builder.HasMany(x => x.JournalEntries)
            .WithOne(x => x.JournalVoucher)
            .HasForeignKey(x => x.JournalVoucherId);
    }
}