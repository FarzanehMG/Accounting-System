namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public record GetSubLegerAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}