using Domain.Common;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Domain.Entities.Accounts;

public sealed class DetailedAccountEntity : BaseEntity<Guid>
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public required JournalEntryType JournalEntryType { get; set; }

    public required Guid SubLedgerAccountId { get; set; }
	public SubLedgerAccountEntity? SubLedgerAccount { get; set; }

    public ICollection<JournalEntryEntity> JournalEntries { get; set; } = new List<JournalEntryEntity>();
}