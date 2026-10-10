using Domain.Enums;

namespace API.Controllers.Models.Accounts;

public record CreateGeneralLedgerAccountApiModel
{
    public required Guid GroupAccountId { get; init; }
    public required string Title { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required bool IsActive { get; init; }
}