namespace Application.Features.Journal.Queries.JournalEntry;

public record GetJournalEntryByIdQuery
{
    public required Guid Id { get; init; }
}