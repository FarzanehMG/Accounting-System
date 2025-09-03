namespace Application.Features.Accounts.Queries.DetailedAccount;

public record GetDetailedAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}