using Domain.Enums;

namespace Application.Features.Journal.Queries.JournalEntry;

public class GetJournalEntryByIdQueryResult
{
    public required Guid Id { get; init; }
    public required int JournalEntryNumber { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? DetailedAccountId { get; init; }
    public required Guid SubLedgerAccountId { get; init; }
    public required Guid JournalVoucherId { get; init; }
}