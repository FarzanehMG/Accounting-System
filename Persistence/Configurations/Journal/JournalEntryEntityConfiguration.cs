using Domain.Entities.Journal;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configurations.Journal;

internal class JournalEntryEntityConfiguration : IEntityTypeConfiguration<JournalEntryEntity>
{
    public void Configure(EntityTypeBuilder<JournalEntryEntity> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JournalEntryNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.JournalEntryType)
            .IsRequired()
            .HasConversion<EnumToStringConverter<JournalEntryType>>();

        builder.Property(c => c.JournalEntryType)
            .HasMaxLength(20);

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2048);

        builder.Property(x => x.SubLedgerAccountId)
            .IsRequired();

        builder.Property(x => x.JournalVoucherId)
            .IsRequired();

        // Index
        builder.HasIndex(x => x.SubLedgerAccountId);
        builder.HasIndex(x => x.JournalVoucherId);
        builder.HasIndex(x => x.JournalEntryType);
        builder.HasIndex(x => x.JournalEntryNumber).IsUnique();

        // Navigation
        builder.HasOne(x => x.JournalVoucher)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.JournalVoucherId);

        builder.HasOne(x => x.SubLedgerAccount)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.SubLedgerAccountId);

        builder.HasOne(x => x.DetailedAccount)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.DetailedAccountId);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.JournalEntryEntity)
            .HasForeignKey(x => x.JournalEntryId);
    }
}