using Application.Common;
using Application.Features.FinancialPeriod.Commands;
using Application.Features.FinancialPeriod.Queries;

namespace Application.Services.FinancialPeriod;

public sealed class FinancialPeriodService(
    CreateFinancialPeriodCommandHandler createHandler,
    UpdateFinancialPeriodCommandHandler updateHandler,
    GetFinancialPeriodByIdQueryHandler getByIdHandler,
    GetFinancialPeriodsQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateFinancialPeriodCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateFinancialPeriodCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetFinancialPeriodByIdQueryResult>> GetByIdAsync(GetFinancialPeriodByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetFinancialPeriodsQueryResult>>> GetAllAsync(GetFinancialPeriodsQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}