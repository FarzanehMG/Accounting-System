using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalEntryEntity : BaseEntity
{
    public required string JournalEntryNumber { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }

    public Guid? DetailedAccountId { get; init; }
    public DetailedAccountEntity? DetailedAccount { get; init; }

    public required Guid SubLedgerAccountId { get; init; }
    public SubLedgerAccountEntity? SubLedgerAccount { get; init; }

    public required Guid JournalVoucherId { get; init; }
    public JournalVoucherEntity? JournalVoucher { get; init; }
}