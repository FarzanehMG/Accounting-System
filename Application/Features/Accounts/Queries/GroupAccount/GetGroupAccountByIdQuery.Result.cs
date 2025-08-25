namespace Application.Features.Accounts.Queries.GroupAccount;

public record GetGroupAccountByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string AccountTitle { get; init; }
}