using Domain.Common;
using Domain.Entities.Journal;

namespace Domain.Entities.Attachment;

public sealed class AttachmentEntity : BaseEntity<Guid>
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public byte[]? FileData { get; init; }

    public required Guid JournalEntryId { get; init; }
    public JournalEntryEntity? JournalEntryEntity { get; init; }
}