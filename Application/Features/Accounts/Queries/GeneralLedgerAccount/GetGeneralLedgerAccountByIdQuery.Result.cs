namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public record GetGeneralLedgerAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}