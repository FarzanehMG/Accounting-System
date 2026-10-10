using Application.Common;
using Application.Repositories.Common;
using Application.Extensions;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class ApprovedJournalVoucherCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(ApprovedJournalVoucherCommand command, CancellationToken cancellationToken)
    {
        // Validator
        var validator = new ApprovedJournalVoucherCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Get journal voucher with entries
        var journalVoucherEntity = await unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(command.JournalVoucherId, cancellationToken);
        if (journalVoucherEntity is null)
            return OperationResult<bool>.NotFoundResult(nameof(command.JournalVoucherId), "This journal voucher was not found.");

        if (journalVoucherEntity.JournalVoucherState != JournalVoucherState.Draft)
            return OperationResult<bool>.FailureResult("JournalVoucherState", "Journal voucher must be in Draft state to approve.");

        if (!journalVoucherEntity.JournalEntries.Any() || journalVoucherEntity.JournalEntries.Count < 2)
            return OperationResult<bool>.FailureResult("Entries", "Cannot approve voucher without at least two journal entries.");

        decimal sumCredit = 0;
        decimal sumDebit = 0;

        foreach (var journalEntry in journalVoucherEntity.JournalEntries)
        {
            // ensure accounts still active
            if (journalEntry.SubLedgerAccountId != Guid.Empty)
            {
                // If navigation not loaded, fetch:
                var subLedgerAccount = journalEntry.SubLedgerAccount ?? await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(journalEntry.SubLedgerAccountId, cancellationToken);
                if (subLedgerAccount is null)
                    return OperationResult<bool>.FailureResult("SubLedgerAccount", "SubLedger account not found.");

                if (!subLedgerAccount.IsActive)
                    return OperationResult<bool>.FailureResult("SubLedgerAccount", $"SubLedger account for entry {journalEntry.JournalEntryNumber} is not active.");
            }

            if (journalEntry.DetailedAccountId != Guid.Empty && journalEntry.DetailedAccountId != null)
            {
                var detailedAccount = journalEntry.DetailedAccount ?? await unitOfWork.DetailedAccountRepository.GetByIdAsync(journalEntry.DetailedAccountId.Value, cancellationToken);
                if (detailedAccount is null)
                    return OperationResult<bool>.FailureResult("DetailedAccount", "Detailed account not found.");

                if (!detailedAccount.IsActive)
                    return OperationResult<bool>.FailureResult("DetailedAccount", $"Detailed account for entry {journalEntry.JournalEntryNumber} is not active.");
            }

            switch (journalEntry.JournalEntryType)
            {
                case JournalEntryType.Credit:
                    sumCredit += journalEntry.Amount;
                    break;
                case JournalEntryType.Debit:
                    sumDebit += journalEntry.Amount;
                    break;
                default:
                    return OperationResult<bool>.FailureResult("JournalEntryType", "Invalid journal entry type.");
            }
        }

        if (sumCredit != sumDebit)
            return OperationResult<bool>.FailureResult("JournalEntryBalance", "Journal entries are not balanced.");

        // Update
        journalVoucherEntity.JournalVoucherState = JournalVoucherState.Posted;
        journalVoucherEntity.ApprovedByUserId = command.ApprovedByUserId;
        journalVoucherEntity.ApprovedTime = DateTime.UtcNow;
        journalVoucherEntity.ModifiedDate = DateTime.UtcNow;

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}