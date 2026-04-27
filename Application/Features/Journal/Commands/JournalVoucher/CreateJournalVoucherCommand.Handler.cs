using Application.Common;
using Application.Contracts.JournalVoucherService;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class CreateJournalVoucherCommandHandler(IUnitOfWork unitOfWork, IJournalVoucherNumberGenerator numberGenerator)
{
    public async Task<OperationResult<bool>> Handle(CreateJournalVoucherCommand command, CancellationToken cancellationToken)
    {
        // Input validation
        var validator = new CreateJournalVoucherCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Check financial period (read-only - do outside transaction)
        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetByIdAsync(command.FinancialPeriodId, cancellationToken);
        if (financialPeriod is null)
            return OperationResult<bool>.NotFoundResult(nameof(command.FinancialPeriodId), "Financial period not found.");

        if (!financialPeriod.IsActive)
            return OperationResult<bool>.FailureResult(nameof(command.FinancialPeriodId), "This financial period is not active.");

        if (command.TransactionTime < financialPeriod.StartDate || command.TransactionTime > financialPeriod.EndDate)
            return OperationResult<bool>.FailureResult(nameof(command.FinancialPeriodId), "Transaction time must be within the selected financial period.");

        // Execute transactional work using UnitOfWork's execution strategy aware method
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Generate Number
            var journalVoucherNumber = await numberGenerator.GenerateAsync(command.FinancialPeriodId, cancellationToken);

            // Create journal voucher header
            var journalVoucherEntity = new JournalVoucherEntity
            {
                JournalVoucherTitle = command.JournalVoucherTitle,
                JournalVoucherNumber = journalVoucherNumber,
                Description = command.Description,
                TransactionTime = command.TransactionTime,
                FinancialPeriodId = command.FinancialPeriodId,
                JournalVoucherState = JournalVoucherState.Draft,
                ReferenceNumber = command.ReferenceNumber,
                CreatedByUserId = command.CreatedByUserId
            };

            await unitOfWork.JournalVoucherRepository.CreateAsync(journalVoucherEntity, cancellationToken);

            return OperationResult<bool>.SuccessResult(true);
        }, cancellationToken);
    }
}