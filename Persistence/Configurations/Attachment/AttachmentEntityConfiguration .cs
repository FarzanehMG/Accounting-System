using Domain.Entities.Attachment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Attachment;

internal class AttachmentEntityConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileData)
            .HasColumnType("varbinary(max)") // store as binary
            .IsRequired(false);

        // Navigation
        builder.HasOne(x => x.JournalEntryEntity)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.JournalEntryId);

        // Index
        builder.HasIndex(x => x.JournalEntryId);
    }
}