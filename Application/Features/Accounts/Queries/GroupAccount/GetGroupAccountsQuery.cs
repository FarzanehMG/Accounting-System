using Domain.Enums;

namespace Application.Features.Accounts.Queries.GroupAccount;

public record GetGroupAccountsQuery
{
    public string? Title { get; init; }
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public JournalEntryType? JournalEntryType { get; init; }
}