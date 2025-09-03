using Domain.Enums;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public record GetDetailedAccountsQuery
{
    public string? Title { get; init; }
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? SubLedgerAccountId { get; init; }
}