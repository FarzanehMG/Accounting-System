namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public record GetGeneralLedgerAccountByIdQuery
{
    public required Guid Id { get; init; }
}