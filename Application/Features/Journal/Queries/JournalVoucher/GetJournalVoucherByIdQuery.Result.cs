using Domain.Enums;

namespace Application.Features.Journal.Queries.JournalVoucher;

public record GetJournalVoucherByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string JournalVoucherTitle { get; init; }
    public required int JournalVoucherNumber { get; init; }
    public required string ReferenceNumber { get; init; }
    public required DateTime TransactionTime { get; init; }
    public string? Description { get; init; }
    public required JournalVoucherState JournalVoucherState { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public Guid? ApprovedByUserId { get; init; }
    public DateTime? ApprovedTime { get; init; }
    public required Guid FinancialPeriodId { get; init; }
}