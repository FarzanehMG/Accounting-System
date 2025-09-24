using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.Accounts;

public sealed class GeneralLedgerAccountEntity : BaseEntity<Guid>
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public required JournalEntryType JournalEntryType { get; set; }

    public required Guid GroupAccountId { get; set; }
	public GroupAccountEntity? GroupAccount { get; set; }

	public ICollection<SubLedgerAccountEntity> SubLedgerAccounts { get; set; } = new List<SubLedgerAccountEntity>();
}