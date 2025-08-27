namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public record GetGeneralAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}