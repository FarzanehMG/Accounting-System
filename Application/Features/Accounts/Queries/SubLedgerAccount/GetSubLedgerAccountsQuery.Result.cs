using Domain.Enums;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public record GetSubLedgerAccountsQueryResult
{
    public required string AccountTitle { get; init; }
    public required string AccountCode { get; init; }
    public required bool IsActive { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required Guid GeneralLedgerAccountId { get; init; }
}