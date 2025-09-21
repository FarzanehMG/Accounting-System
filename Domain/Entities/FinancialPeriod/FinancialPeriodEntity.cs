using Domain.Common;
using Domain.Entities.Journal;

namespace Domain.Entities.FinancialPeriod;

public sealed class FinancialPeriodEntity : BaseEntity
{
    public required string FinancialPeriodTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    public ICollection<JournalVoucherEntity> JournalVouchers { get; set; } = new List<JournalVoucherEntity>();
}