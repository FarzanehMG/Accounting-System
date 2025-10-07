using Domain.Enums;

namespace API.Controllers.Models.Journal;

public record FilterJournalEntriesApiModel
{
    public string? JournalEntryNumber { get; init; }
    public JournalEntryType? JournalEntryType { get; init; }
    public decimal? Amount { get; init; }
    public Guid? DetailedAccountId { get; init; }
    public Guid? SubLedgerAccountId { get; init; }
    public Guid? JournalVoucherId { get; init; }
}