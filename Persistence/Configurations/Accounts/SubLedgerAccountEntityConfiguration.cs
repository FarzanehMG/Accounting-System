using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configurations.Accounts;

internal class SubLedgerAccountEntityConfiguration : IEntityTypeConfiguration<SubLedgerAccountEntity>
{
    public void Configure(EntityTypeBuilder<SubLedgerAccountEntity> builder)
    {
        builder.ToTable("SubLedgerAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountDescription)
            .HasMaxLength(2048);

        builder.Property(x => x.JournalEntryType)
            .IsRequired()
            .HasConversion<EnumToStringConverter<JournalEntryType>>();

        builder.Property(x => x.JournalEntryType)
            .HasMaxLength(20);

        builder.Property(x => x.GeneralLedgerAccountId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.AccountCode).IsUnique();
        builder.HasIndex(x => x.AccountTitle);
        builder.HasIndex(x => x.JournalEntryType);
        builder.HasIndex(x => x.IsActive);

        // Navigation
        builder.HasMany(x => x.DetailedAccounts)
            .WithOne(x => x.SubLedgerAccount)
            .HasForeignKey(x => x.SubLedgerAccountId);

        builder.HasOne(x => x.GeneralLedgerAccount)
            .WithMany(x => x.SubLedgerAccounts)
            .HasForeignKey(x => x.GeneralLedgerAccountId);

        builder.HasMany(x => x.JournalEntries)
            .WithOne(x => x.SubLedgerAccount)
            .HasForeignKey(x => x.SubLedgerAccountId);
    }
}