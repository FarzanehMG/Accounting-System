using Application.Common;
using Application.Features.Accounts.Commands.GeneralLedgerAccount;
using Application.Features.Accounts.Queries.GeneralLedgerAccount;

namespace Application.Services.Accounts;

public sealed class GeneralLedgerAccountService(
    CreateGeneralLedgerAccountCommandHandler createHandler,
    UpdateGeneralLedgerAccountCommandHandler updateHandler,
    GetGeneralLedgerAccountByIdQueryHandler getByIdHandler,
    GetGeneralLedgerAccountsQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateGeneralLedgerAccountCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateGeneralLedgerAccountCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetGeneralLedgerAccountByIdQueryResult>> GetByIdAsync(GetGeneralLedgerAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetGeneralLedgerAccountsQueryResult>>> GetAllAsync(GetGeneralLedgerAccountsQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}