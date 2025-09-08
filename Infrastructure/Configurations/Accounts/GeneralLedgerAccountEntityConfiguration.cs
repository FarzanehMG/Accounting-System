using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations.Accounts;

internal class GeneralLedgerAccountEntityConfiguration : IEntityTypeConfiguration<GeneralLedgerAccountEntity>
{
    public void Configure(EntityTypeBuilder<GeneralLedgerAccountEntity> builder)
    {
        builder.ToTable("GeneralLedgerAccounts");

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
        builder.HasMany(x => x.SubLedgerAccounts)
            .WithOne(x => x.GeneralLedgerAccount)
            .HasForeignKey(x => x.GeneralLedgerAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x=>x.GroupAccount)
            .WithMany(x=>x.GeneralLedgerAccounts)
            .HasForeignKey(x=>x.GroupAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}