namespace Application.Features.Accounts.Queries.GeneralAccount;

public record GetGeneralLedgerAccountByIdQuery
{
    public required Guid Id { get; init; }
}