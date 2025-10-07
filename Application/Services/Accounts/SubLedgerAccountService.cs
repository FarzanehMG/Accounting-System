using Application.Common;
using Application.Features.Accounts.Commands.SubLedgerAccount;
using Application.Features.Accounts.Queries.SubLedgerAccount;

namespace Application.Services.Accounts;

public sealed class SubLedgerAccountService(
    CreateSubLedgerAccountCommandHandler createHandler,
    UpdateSubLedgerAccountCommandHandler updateHandler,
    GetSubLedgerAccountByIdQueryHandler getByIdHandler,
    GetSubLedgerAccountsQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateSubLedgerAccountCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateSubLedgerAccountCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetSubLedgerAccountByIdQueryResult>> GetByIdAsync(GetSubLedgerAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetSubLedgerAccountsQueryResult>>> GetAllAsync(GetSubLedgerAccountsQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}