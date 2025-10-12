using Application.Contracts.FileService.Models;
using Domain.Enums;

namespace API.Controllers.Models.Journal;

public record CreateJournalEntryApiModel
{
    public required List<JournalEntryApiModel> Entries { get; set; }
}

public record JournalEntryApiModel
{
    public required JournalEntryType JournalEntryType { get; init; }
    public required decimal Amount { get; init; }
    public string? Description { get; init; }
    public required Guid SubLedgerAccountId { get; init; }
    public Guid? DetailedAccountId { get; init; }
    public required Guid JournalVoucherId { get; init; }
    public required List<SaveFileModel> Attachments { get; init; } = [];
}