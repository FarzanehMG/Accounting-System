using Domain.Common;

namespace Domain.Entities.Balance;

public sealed class AccountBalanceEntity : BaseEntity<Guid>
{
    public required Guid AccountId { get; set; }
    public required Guid FinancialPeriodId { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    // Closing balance = TotalDebit - TotalCredit
    public decimal ClosingBalance { get; set; }
}