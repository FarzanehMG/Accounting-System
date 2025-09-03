namespace Application.Features.Accounts.Queries.DetailedAccount;

public record GetDetailedAccountByIdQuery
{
    public required Guid Id { get; init; }
}