using Domain.Enums;

namespace Application.Features.Journal.Queries.JournalVoucher.Dtos;

public record JournalVoucherFilter
{
    public string? JournalVoucherTitle { get; init; }
    public string? JournalVoucherNumber { get; init; }
    public string? ReferenceNumber { get; init; }
    public DateTime? TransactionTime { get; init; }
    public JournalVoucherState? JournalVoucherState { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public Guid? ApprovedByUserId { get; init; }
    public DateTime? ApprovedTime { get; init; }
    public Guid? FinancialPeriodId { get; init; }
}