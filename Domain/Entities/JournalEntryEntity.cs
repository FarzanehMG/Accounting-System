using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalEntryEntity : BaseEntity
{
    public required string JournalEntryNumber { get; set; }
    public required JournalEntryType JournalEntryType { get; set; }
    public required decimal Amount { get; set; }
    public required string Description { get; set; }

    public required Guid DetailedAccountId { get; set; }
    public DetailedAccountEntity? DetailedAccount { get; set; }

    public required Guid JournalVoucherId { get; set; }
    public JournalVoucherEntity? JournalVoucher { get; set; }
}