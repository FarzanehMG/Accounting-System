using Domain.Enums;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public record CreateDetailedAccountCommand
{
    public required string Title { get; init; }
    public required string Code { get; init; }
    public required string? Description { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required bool IsActive { get; init; }
    public required Guid SubLedgerAccountId { get; init; }
}