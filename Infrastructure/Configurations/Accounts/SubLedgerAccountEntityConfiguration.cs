using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations.Accounts;

internal class SubLedgerAccountEntityConfiguration : IEntityTypeConfiguration<SubLedgerAccountEntity>
{
    public void Configure(EntityTypeBuilder<SubLedgerAccountEntity> builder)
    {
        builder.ToTable("SubLedgerAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountTitle)
            .HasMaxLength(100);

        builder.Property(x => x.AccountCode)
            .HasMaxLength(100);

        builder.Property(x => x.AccountDescription)
            .HasMaxLength(2048);


        builder.Property(c => c.JournalEntryType)
            .HasConversion<EnumToStringConverter<JournalEntryType>>();

        builder.Property(c => c.JournalEntryType)
            .HasMaxLength(20);

        // Index
        builder.HasIndex(c => c.AccountTitle);
        builder.HasIndex(c => c.JournalEntryType);
        builder.HasIndex(c => c.IsActive);

        // Navigation
        builder.HasMany(x => x.DetailedAccounts)
            .WithOne(x => x.SubLedgerAccount)
            .HasForeignKey(x => x.SubLedgerAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x=>x.GeneralLedgerAccount)
            .WithMany(x=>x.SubLedgerAccounts)
            .HasForeignKey(x=>x.GeneralLedgerAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}