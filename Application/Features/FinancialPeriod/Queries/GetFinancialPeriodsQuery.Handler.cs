using Application.Common;
using Application.Converters.FinancialPeriod;
using Application.Extensions;
using Application.Features.FinancialPeriod.Queries.Dtos;
using Application.Repositories.Common;

namespace Application.Features.FinancialPeriod.Queries;

public class GetFinancialPeriodsQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetFinancialPeriodsQueryResult>>> Handle(GetFinancialPeriodsQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetFinancialPeriodsQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetFinancialPeriodsQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var financialPeriodFilter = new FinancialPeriodFilter
        {
           Title = query.Title,
           IsActive = query.IsActive,
           StartDate = query.StartDate,
           EndDate = query.EndDate
        };
        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetAllAsync(financialPeriodFilter, cancellationToken);

        if (!financialPeriod.Any())
            return OperationResult<IReadOnlyList<GetFinancialPeriodsQueryResult>>.NotFoundResult(nameof(GetFinancialPeriodsQueryResult), "Financial period not found.");

        return OperationResult<IReadOnlyList<GetFinancialPeriodsQueryResult>>
            .SuccessResult(financialPeriod.ToDto());
    }
}