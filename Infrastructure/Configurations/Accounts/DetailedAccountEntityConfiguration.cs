using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations.Accounts;

internal class DetailedAccountEntityConfiguration : IEntityTypeConfiguration<DetailedAccountEntity>
{
    public void Configure(EntityTypeBuilder<DetailedAccountEntity> builder)
    {
        builder.ToTable("DetailedAccounts");

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

        builder.Property(x => x.SubLedgerAccountId)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.AccountCode).IsUnique();
        builder.HasIndex(x => x.AccountTitle);
        builder.HasIndex(x => x.JournalEntryType);
        builder.HasIndex(x => x.IsActive);

        // Navigation
        builder.HasOne(x => x.SubLedgerAccount)
            .WithMany(x => x.DetailedAccounts)
            .HasForeignKey(x => x.SubLedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.JournalEntries)
            .WithOne(x => x.DetailedAccount)
            .HasForeignKey(x => x.DetailedAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}