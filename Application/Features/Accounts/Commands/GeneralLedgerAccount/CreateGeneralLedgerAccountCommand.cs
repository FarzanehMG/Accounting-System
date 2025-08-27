using Domain.Enums;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public record CreateGeneralLedgerAccountCommand
{
    public required string Title { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required bool IsActive { get; init; }
    public required Guid GroupAccountId { get; init; }
}