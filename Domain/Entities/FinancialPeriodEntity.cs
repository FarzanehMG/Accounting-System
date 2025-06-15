using Domain.Common;

namespace Domain.Entities;

public sealed class FinancialPeriodEntity : BaseEntity
{
    public required string FinancialPeriodTitle { get; set; }
    public required bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<JournalVoucherEntity> JournalVouchers { get; set; } = new List<JournalVoucherEntity>();
}