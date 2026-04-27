using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Model;

namespace Persistence.Configurations.Journal;

internal sealed class JournalEntrySequenceConfiguration : IEntityTypeConfiguration<JournalEntrySequence>
{
    public void Configure(EntityTypeBuilder<JournalEntrySequence> builder)
    {
        builder.ToTable("JournalEntrySequences");

        builder.HasKey(x => x.JournalVoucherId);

        builder.Property(x => x.CurrentNumber)
            .IsRequired()
            .HasDefaultValue(0);
    }
}