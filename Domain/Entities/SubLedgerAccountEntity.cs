using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class SubLedgerAccountEntity : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }
    public required bool IsActive { get; set; } = true;
    public required JournalEntryType JournalEntryType { get; set; }

    public Guid GeneralLedgerAccountId { get; set; }
	public GeneralLedgerAccountEntity? GeneralLedgerAccount { get; set; }

	public ICollection<DetailedAccountEntity> DetailedAccounts { get; set; } = new List<DetailedAccountEntity>();
}