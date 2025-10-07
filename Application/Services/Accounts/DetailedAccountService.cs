using Application.Common;
using Application.Features.Accounts.Commands.DetailedAccount;
using Application.Features.Accounts.Queries.DetailedAccount;

namespace Application.Services.Accounts;

public sealed class DetailedAccountService(
    CreateDetailedAccountCommandHandler createHandler,
    UpdateDetailedAccountCommandHandler updateHandler,
    GetDetailedAccountByIdQueryHandler getByIdHandler,
    GetDetailedAccountsQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateDetailedAccountCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateDetailedAccountCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetDetailedAccountByIdQueryResult>> GetByIdAsync(GetDetailedAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetDetailedAccountsQueryResult>>> GetAllAsync(GetDetailedAccountsQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}