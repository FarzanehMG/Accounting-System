using Application.Common;
using Application.Contracts.FileService.Models;

namespace Application.Features.Journal.Commands.JournalEntry;

public record UpdateJournalEntryCommand
{
    public required Guid JournalEntryId { get; init; }
    public Patch<decimal>? Amount { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<Guid>? SubLedgerAccountId { get; init; }
    public Patch<Guid>? DetailedAccountId { get; init; }
    public Patch<List<SaveFileModel>>? RemoveAttachments { get; init; }
    public Patch<List<SaveFileModel>>? NewAttachments { get; init; }
}