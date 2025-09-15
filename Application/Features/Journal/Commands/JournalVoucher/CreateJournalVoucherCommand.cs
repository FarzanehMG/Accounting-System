namespace Application.Features.Journal.Commands.JournalVoucher;

public record CreateJournalVoucherCommand
{
    public required string JournalVoucherTitle { get; init; }
    public required string ReferenceNumber { get; init; }
    public required DateTime TransactionTime { get; init; }
    public string? Description { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required Guid FinancialPeriodId { get; init; }
}