using Domain.Common;

namespace Domain.Entities;

public sealed class GeneralLedgerAccount : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }

	public Guid GroupAccountId { get; set; }
	public GroupAccount? GroupAccount { get; set; }

	public ICollection<SubLedgerAccount> SubLedgerAccounts { get; set; } = new List<SubLedgerAccount>();
}