using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalVoucherEntity : BaseEntity
{
    public required string JournalVoucherTitle { get; set; }
    public required string JournalVoucherNumber { get; set; }
    public required string ReferenceNumber { get; set; }
    public required DateTime TransactionTime { get; set; }
    public required string Description { get; set; }
    public required JournalVoucherState JournalVoucherState { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public required Guid CompanyFinancialPeriodId { get; set; }
    public FinancialPeriodEntity? FinancialPeriod{ get; set; }

    public ICollection<JournalEntryEntity> JournalEntries { get; set; } = new List<JournalEntryEntity>();

    //public required string CurrencyId { get; set; }
}