using Application.Common;
using Application.Repositories.Common;
using Application.Extensions;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class CreateJournalVoucherCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateJournalVoucherCommand command, CancellationToken cancellationToken)
    {
        // Validator
        var validator = new CreateJournalVoucherCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Check financial period
        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(command.FinancialPeriodId, cancellationToken);
        if (financialPeriod is null)
            return OperationResult<bool>.NotFoundResult(nameof(command.FinancialPeriodId), "Financial period not found.");

        if (!financialPeriod.IsActive)
            return OperationResult<bool>.FailureResult(nameof(command.FinancialPeriodId), "This financial period is not active.");

        if (command.TransactionTime < financialPeriod.StartDate || command.TransactionTime > financialPeriod.EndDate)
            return OperationResult<bool>.FailureResult(nameof(command.FinancialPeriodId), "Transaction time must be within the selected financial period.");

        // Generate journal voucher Number (incremental per last JournalVoucher)
        var lastJournalVoucher = await unitOfWork.JournalVoucherRepository.GetLastJournalVoucherAsync(command.FinancialPeriodId, cancellationToken);
        var journalVoucherNumber = (lastJournalVoucher is null) ? "1" : (int.Parse(lastJournalVoucher.JournalVoucherNumber) + 1).ToString();

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
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}