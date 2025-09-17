namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public record GetSubLedgerAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}