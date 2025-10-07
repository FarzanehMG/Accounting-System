using Application.Common;
using Application.Features.Accounts.Commands.GroupAccount;
using Application.Features.Accounts.Queries.GroupAccount;

namespace Application.Services.Accounts;

public class GroupAccountService(
    CreateGroupAccountCommandHandler createHandler,
    UpdateGroupAccountCommandHandler updateHandler,
    GetGroupAccountByIdQueryHandler getByIdHandler,
    GetGroupAccountsQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateGroupAccountCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateGroupAccountCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetGroupAccountByIdQueryResult>> GetByIdAsync(GetGroupAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetGroupAccountsQueryResult>>> GetAllAsync(GetGroupAccountsQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}