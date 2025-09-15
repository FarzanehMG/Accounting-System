using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class UpdateJournalVoucherCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateJournalVoucherCommand command, CancellationToken cancellationToken)
    {
        // Validator
        var validator = new UpdateJournalVoucherCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Load journal voucher
        var journalVoucherEntity = await unitOfWork.JournalVoucherRepository.GetByIdAsync(command.JournalVoucherId, cancellationToken);
        if (journalVoucherEntity is null)
            return OperationResult<bool>.FailureResult("JournalVoucher", "Journal voucher not found.");

        // Only Draft journal vouchers can be updated
        if (journalVoucherEntity.JournalVoucherState != JournalVoucherState.Draft)
            return OperationResult<bool>.FailureResult("JournalVoucher", "Cannot update entry of a non-Draft voucher.");

        // Financial period must be open
        if (journalVoucherEntity.FinancialPeriod is { IsActive: false })
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Financial period is closed. Cannot update journal voucher.");

        // Apply changes
        if (!string.IsNullOrWhiteSpace(command.JournalVoucherTitle?.Value))
            journalVoucherEntity.JournalVoucherTitle = command.JournalVoucherTitle.Value;

        if (command.TransactionTime != null && journalVoucherEntity.FinancialPeriod != null && 
            (command.TransactionTime.Value < journalVoucherEntity.FinancialPeriod.StartDate || command.TransactionTime.Value > journalVoucherEntity.FinancialPeriod.EndDate))
        {
            return OperationResult<bool>.FailureResult(
                "FinancialPeriodId", $"Transaction time must be within the selected financial period ({journalVoucherEntity.FinancialPeriod.StartDate:yyyy-MM-dd} - {journalVoucherEntity.FinancialPeriod.EndDate:yyyy-MM-dd}).");
        }

        if (command.TransactionTime != null) 
            journalVoucherEntity.TransactionTime = command.TransactionTime.Value;

        if (!string.IsNullOrWhiteSpace(command.ReferenceNumber?.Value))
            journalVoucherEntity.ReferenceNumber = command.ReferenceNumber.Value;

        if (command.Description?.Value != null)
            journalVoucherEntity.Description = command.Description.Value;

        if (unitOfWork.HasChanges(journalVoucherEntity))
        {
            journalVoucherEntity.ModifiedDate = DateTime.UtcNow;
            await unitOfWork.CommitAsync(cancellationToken);
        }
        return OperationResult<bool>.SuccessResult(true);
    }
}