using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class DetailedAccountEntity : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }
    public required bool IsActive { get; set; } = true;
    public required JournalEntryType JournalEntryType { get; set; }

    public required Guid SubLedgerAccountId { get; set; }
	public SubLedgerAccountEntity? SubLedgerAccount { get; set; }

    public ICollection<JournalVoucherEntity> JournalVouchers { get; set; } = new List<JournalVoucherEntity>();
}