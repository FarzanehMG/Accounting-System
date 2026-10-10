namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public record GetSubLedgerAccountByIdQuery
{
    public required Guid Id { get; init; }
}