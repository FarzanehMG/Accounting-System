using Application.Common;
using Application.Converters.FinancialPeriod;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.FinancialPeriod.Queries;

public class GetFinancialPeriodByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetFinancialPeriodByIdQueryResult>> Handle(GetFinancialPeriodByIdQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetFinancialPeriodByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetFinancialPeriodByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetByIdAsync(query.Id, cancellationToken);

        if (financialPeriod is null)
            return OperationResult<GetFinancialPeriodByIdQueryResult>.NotFoundResult(nameof(GetFinancialPeriodByIdQuery.Id), "Financial period not found.");

        return OperationResult<GetFinancialPeriodByIdQueryResult>.SuccessResult(financialPeriod.ToDto());
    }
}