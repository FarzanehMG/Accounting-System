namespace Application.Features.Accounts.Queries.GroupAccount;

public record GetGroupAccountByIdQuery
{
    public required Guid Id { get; init; }
}