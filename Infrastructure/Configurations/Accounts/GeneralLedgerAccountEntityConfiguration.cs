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

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.GroupAccountId)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.AccountCode).IsUnique();
        builder.HasIndex(x => x.AccountTitle);
        builder.HasIndex(x => x.JournalEntryType);
        builder.HasIndex(x => x.IsActive);

        // Navigation
        builder.HasMany(x => x.SubLedgerAccounts)
            .WithOne(x => x.GeneralLedgerAccount)
            .HasForeignKey(x => x.GeneralLedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GroupAccount)
            .WithMany(x => x.GeneralLedgerAccounts)
            .HasForeignKey(x => x.GroupAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}