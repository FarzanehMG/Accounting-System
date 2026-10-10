using Domain.Enums;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public record GetSubLedgerAccountsQuery
{
    public string? Title { get; init; }
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? GeneralLedgerAccountId { get; init; }
}