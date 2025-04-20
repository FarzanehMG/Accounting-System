using Domain.Common;

namespace Domain.Entities;

public sealed class SubLedgerAccount : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }

	public required Guid GeneralLedgerAccountId { get; set; }
	public GeneralLedgerAccount? GeneralLedgerAccount { get; set; }

	public ICollection<DetailedAccount> DetailedAccounts { get; set; } = new List<DetailedAccount>();
}