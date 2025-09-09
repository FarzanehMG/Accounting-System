using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.FinancialPeriod.Commands;

public class UpdateFinancialPeriodCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateFinancialPeriodCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateFinancialPeriodCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Retrieve the existing financial period
        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(command.FinancialPeriodId, cancellationToken);

        if (financialPeriod is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateFinancialPeriodCommand), "Financial period not found.");

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(command.FinancialPeriodTitle?.Value))
            financialPeriod.FinancialPeriodTitle = command.FinancialPeriodTitle.Value;

        // Business Rule: Ensure not overlapping dates
        var newStartDate = command.StartDate ?? financialPeriod.StartDate;
        var newEndDate = command.EndDate ?? financialPeriod.EndDate;

        if (command.StartDate is not null && financialPeriod.JournalVouchers.Any())
            return OperationResult<bool>.FailureResult(nameof(command.StartDate), "Cannot change StartDate because some journal entries already exist.");

        if (newEndDate.Value <= newStartDate.Value)
            return OperationResult<bool>.FailureResult(nameof(command.EndDate), "End date must be later than start date.");

        var overlappingFinancialPeriod = await unitOfWork.FinancialPeriodRepository
            .GetOverlappingFinancialPeriodAsync(cancellationToken, newStartDate, newEndDate);

        if (overlappingFinancialPeriod is not null)
            return OperationResult<bool>.FailureResult("Date", "The selected period overlaps with an existing financial period.");

        financialPeriod.StartDate = newStartDate;
        financialPeriod.EndDate = newEndDate;

        if (unitOfWork.HasChanges(financialPeriod))
        {
            financialPeriod.ModifiedDate = DateTime.UtcNow;
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return OperationResult<bool>.SuccessResult(true);
    }
}