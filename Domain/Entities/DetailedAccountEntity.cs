using Domain.Common;

namespace Domain.Entities;

public sealed class DetailedAccountEntity : BaseEntity
{
	public required string AccountTitle { get; set; }
	public required string AccountCode { get; set; }
	public string? AccountDescription { get; set; }

	public required Guid SubLedgerAccountId { get; set; }
	public SubLedgerAccountEntity? SubLedgerAccount { get; set; }
}