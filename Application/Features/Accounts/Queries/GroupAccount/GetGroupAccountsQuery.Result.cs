using Domain.Enums;

namespace Application.Features.Accounts.Queries.GroupAccount;

public record GetGroupAccountsQueryResult
{
    public required string AccountTitle { get; init; }
    public required string AccountCode { get; init; }
    public required string? AccountDescription { get; init; }
    public required bool IsActive { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
}