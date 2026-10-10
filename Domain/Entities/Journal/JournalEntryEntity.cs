using Domain.Common;
using Domain.Entities.Accounts;
using Domain.Entities.Attachment;
using Domain.Enums;

namespace Domain.Entities.Journal;

public sealed class JournalEntryEntity : BaseEntity<Guid>
{
    public int JournalEntryNumber { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }

    public Guid? DetailedAccountId { get; set; }
    public DetailedAccountEntity? DetailedAccount { get; init; }

    public required Guid SubLedgerAccountId { get; set; }
    public SubLedgerAccountEntity? SubLedgerAccount { get; init; }

    public required Guid JournalVoucherId { get; set; }
    public JournalVoucherEntity? JournalVoucher { get; init; }

    public ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
}