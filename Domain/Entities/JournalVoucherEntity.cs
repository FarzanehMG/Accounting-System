using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalVoucherEntity : BaseEntity
{
    public required string JournalVoucherTitle { get; init; }
    public string JournalVoucherNumber { get; init; } = null!;
    public required string ReferenceNumber { get; init; }
    public required DateTime TransactionTime { get; init; }
    public required string Description { get; init; }
    public required JournalVoucherState JournalVoucherState { get; set; }
    public required Guid CreatedByUserId { get; init; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedTime { get; set; }

    public required Guid FinancialPeriodId { get; init; }
    public FinancialPeriodEntity? FinancialPeriod{ get; init; }

    public ICollection<JournalEntryEntity> JournalEntries { get; init; } = new List<JournalEntryEntity>();
}