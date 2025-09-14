using Application.Common;

namespace Application.Features.Journal.Commands.JournalEntry;

public record UpdateJournalEntryCommand
{
    public required Guid JournalEntryId { get; init; }
    public Patch<decimal>? Amount { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<Guid>? SubLedgerAccountId { get; init; }
    public Patch<Guid>? DetailedAccountId { get; init; }
}