namespace API.Controllers.Models.Journal;

public record CreateJournalVoucherApiModel
{
    public required string JournalVoucherTitle { get; init; }
    public required string ReferenceNumber { get; init; }
    public required DateTime TransactionTime { get; init; }
    public string? Description { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required Guid FinancialPeriodId { get; init; }
}