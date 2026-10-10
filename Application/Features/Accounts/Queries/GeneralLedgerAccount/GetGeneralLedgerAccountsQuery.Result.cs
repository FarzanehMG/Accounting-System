using Domain.Enums;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public record GetGeneralLedgerAccountsQueryResult
{
    public required string AccountTitle { get; init; }
    public required string AccountCode { get; init; }
    public required bool IsActive { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required Guid GroupAccountId{ get; init; }
}