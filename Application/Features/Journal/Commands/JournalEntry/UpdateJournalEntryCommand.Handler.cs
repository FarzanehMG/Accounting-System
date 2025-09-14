using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalEntry;

public class UpdateJournalEntryCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateJournalEntryCommand command, CancellationToken cancellationToken)
    {
        // Validator
        var validator = new UpdateJournalEntryCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Load journal entry and journal voucher
        var journalEntryEntity = await unitOfWork.JournalEntryRepository.GetByIdAsync(command.JournalEntryId, cancellationToken);
        if (journalEntryEntity is null)
            return OperationResult<bool>.NotFoundResult(nameof(command.JournalEntryId), "Journal entry not found.");

        var journalVoucherEntity = await unitOfWork.JournalVoucherRepository.GetByIdAsync(journalEntryEntity.JournalVoucherId, cancellationToken);
        if (journalVoucherEntity is null)
            return OperationResult<bool>.FailureResult("JournalVoucher", "Parent voucher not found.");

        // Only Draft journal vouchers can be updated
        if (journalVoucherEntity.JournalVoucherState != JournalVoucherState.Draft)
            return OperationResult<bool>.FailureResult("JournalVoucher", "Cannot update entry of a non-Draft voucher.");

        // Financial period must be open
        if (journalVoucherEntity.FinancialPeriod is { IsActive: false })
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Financial period is closed. Cannot update entry.");

        // Apply changes
        if (command.Description?.Value != null)
            journalEntryEntity.Description = command.Description.Value;

        if (command.Amount != null)
        {
            if (command.Amount.Value <= 0)
                return OperationResult<bool>.FailureResult(nameof(command.Amount), "Amount must be greater than zero.");
            journalEntryEntity.Amount = command.Amount.Value;
        }

        if (command.SubLedgerAccountId != null)
        {
            var subLedger = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(command.SubLedgerAccountId.Value, cancellationToken);
            if (subLedger is null)
                return OperationResult<bool>.NotFoundResult(nameof(command.SubLedgerAccountId), "SubLedger account not found.");

            if (!subLedger.IsActive)
                return OperationResult<bool>.FailureResult(nameof(command.SubLedgerAccountId), "SubLedger account is inactive.");

            if (subLedger.JournalEntryType != journalEntryEntity.JournalEntryType)
                return OperationResult<bool>.FailureResult("JournalEntryType", "Entry type does not match subLedger account.");

            journalEntryEntity.SubLedgerAccountId = command.SubLedgerAccountId.Value;
        }

        if (command.DetailedAccountId != null)
        {
            var detailed = await unitOfWork.DetailedAccountRepository.GetByIdAsync(command.DetailedAccountId.Value, cancellationToken);
            if (detailed is null)
                return OperationResult<bool>.NotFoundResult(nameof(command.DetailedAccountId), "Detailed account not found.");

            if (!detailed.IsActive)
                return OperationResult<bool>.FailureResult(nameof(command.DetailedAccountId), "Detailed account is inactive.");

            if (detailed.JournalEntryType != journalEntryEntity.JournalEntryType)
                return OperationResult<bool>.FailureResult("JournalEntryType", "Entry type does not match detailed account.");

            journalEntryEntity.DetailedAccountId = command.DetailedAccountId.Value;
        }

        if (unitOfWork.HasChanges(journalEntryEntity))
        {
            journalEntryEntity.ModifiedDate = DateTime.UtcNow;
            await unitOfWork.CommitAsync(cancellationToken);
        }
        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}