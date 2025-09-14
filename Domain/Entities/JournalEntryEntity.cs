using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalEntryEntity : BaseEntity
{
    public required string JournalEntryNumber { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required decimal Amount { get; set; }
    public required string Description { get; set; }

    public Guid? DetailedAccountId { get; set; }
    public DetailedAccountEntity? DetailedAccount { get; init; }

    public required Guid SubLedgerAccountId { get; set; }
    public SubLedgerAccountEntity? SubLedgerAccount { get; init; }

    public required Guid JournalVoucherId { get; set; }
    public JournalVoucherEntity? JournalVoucher { get; init; }
}