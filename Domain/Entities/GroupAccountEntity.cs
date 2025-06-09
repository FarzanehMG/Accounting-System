using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class GroupAccountEntity : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public required JournalEntryType JournalEntryType { get; set; }

    public ICollection<GeneralLedgerAccountEntity> GeneralLedgerAccounts { get; set; } = new List<GeneralLedgerAccountEntity>();
}