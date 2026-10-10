using Domain.Enums;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public record GetGeneralLedgerAccountsQuery
{
    public string? Title { get; init; }
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? GroupAccountId { get; init; }
}