using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Model;

namespace Persistence.Configurations.Journal;

internal sealed class JournalVoucherSequenceConfiguration : IEntityTypeConfiguration<JournalVoucherSequence>
{
    public void Configure(EntityTypeBuilder<JournalVoucherSequence> builder)
    {
        builder.ToTable("JournalVoucherSequences");

        builder.HasKey(x => x.FinancialPeriodId);

        builder.Property(x => x.CurrentNumber)
            .IsRequired()
            .HasDefaultValue(0)
            .ValueGeneratedNever();
    }
}