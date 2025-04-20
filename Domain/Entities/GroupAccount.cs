using Domain.Common;

namespace Domain.Entities;

public sealed class GroupAccount : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }

	//public Guid CompanyFinancialPeriodId { get; set; }

	public ICollection<GeneralLedgerAccount> GeneralLedgerAccounts { get; set; } = new List<GeneralLedgerAccount>();
}