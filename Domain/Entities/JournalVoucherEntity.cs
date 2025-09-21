using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class JournalVoucherEntity : BaseEntity
{
    public required string JournalVoucherTitle { get; set; }
    public string JournalVoucherNumber { get; init; } = null!;
    public required string ReferenceNumber { get; set; }
    public required DateTime TransactionTime { get; set; }
    public string? Description { get; set; } = null!;
    public required JournalVoucherState JournalVoucherState { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedTime { get; set; }

    public required Guid FinancialPeriodId { get; set; }
    public FinancialPeriodEntity? FinancialPeriod{ get; init; }

    public ICollection<JournalEntryEntity> JournalEntries { get; init; } = new List<JournalEntryEntity>();
}   