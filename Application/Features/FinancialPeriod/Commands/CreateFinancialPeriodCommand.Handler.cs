using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.FinancialPeriod.Commands;

public class CreateFinancialPeriodCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateFinancialPeriodCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateFinancialPeriodCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        if (command.EndDate <= command.StartDate)
            return OperationResult<bool>.FailureResult(nameof(command.StartDate), "The end date must be later than the start date of the financial period.");

        var overlappingFinancialPeriod = await unitOfWork.FinancialPeriodRepository.GetOverlappingFinancialPeriodAsync(cancellationToken, command.StartDate, command.EndDate);

        if (overlappingFinancialPeriod is not null)
            return OperationResult<bool>.FailureResult(nameof(command.EndDate), "The selected period overlaps with an existing financial period.");

        var activeFinancialPeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);

        if(activeFinancialPeriod is not null && activeFinancialPeriod.IsActive)
            return OperationResult<bool>.FailureResult("IsActive", "You must close the current active financial period before creating a new one.");

        var financialPeriodEntity = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = command.FinancialPeriodTitle,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
        };

        await unitOfWork.FinancialPeriodRepository.CreateAsync(financialPeriodEntity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}