namespace Persistence.Model;

public class JournalVoucherSequence
{
    public Guid FinancialPeriodId { get; set; }
    public int CurrentNumber { get; set; }
}