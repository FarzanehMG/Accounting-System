using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalEntry;

public class CreateJournalEntryCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(List<CreateJournalEntryCommand> commands, CancellationToken cancellationToken)
    {
        if (!commands.Any() || commands.Count < 2)
            return OperationResult<bool>.FailureResult("Entries", "At least two journal entry is required.");

        // Ensure all commands target the same JournalVoucherId
        var voucherId = commands.First().JournalVoucherId;
        if (commands.Any(c => c.JournalVoucherId != voucherId))
            return OperationResult<bool>.FailureResult("JournalVoucherId", "All entries must belong to the same journal voucher.");

        // Business Rule: Entries must be balanced (Debit = Credit)
        var debitTotal = commands
            .Where(c => c.JournalEntryType == JournalEntryType.Debit)
            .Sum(c => c.Amount);

        var creditTotal = commands
            .Where(c => c.JournalEntryType == JournalEntryType.Credit)
            .Sum(c => c.Amount);

        if (debitTotal != creditTotal)
            return OperationResult<bool>.FailureResult("Entries", "Journal entries are not balanced. Debit and Credit totals must match.");

        // Generate Entry Number (incremental per JournalVoucher)
        var lastEntry = await unitOfWork.JournalEntryRepository.GetLastEntryByVoucherIdAsync(voucherId, cancellationToken);

        var lastEntryNum = 0;
        if (lastEntry is not null)
            int.TryParse(lastEntry.JournalEntryNumber, out lastEntryNum);

        // Create entries and commit once at the end (atomic)
        foreach (var command in commands)
        {
            // Validator
            var validator = new CreateJournalEntryCommandValidator();
            var validateResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validateResult.IsValid)
                return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

            // Check Journal Voucher
            var journalVoucherEntity = await unitOfWork.JournalVoucherRepository.GetByIdAsync(command.JournalVoucherId, cancellationToken);
            if (journalVoucherEntity is null)
                return OperationResult<bool>.NotFoundResult(nameof(command.JournalVoucherId), "The journal voucher was not found.");

            if (journalVoucherEntity.JournalVoucherState != JournalVoucherState.Draft)
                return OperationResult<bool>.FailureResult(nameof(journalVoucherEntity.JournalVoucherState), "Cannot add entries to a non-Draft voucher.");

            // Check SubLedger account
            var subLedgerAccountEntity = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(command.SubLedgerAccountId, cancellationToken);
            if (subLedgerAccountEntity is null)
                return OperationResult<bool>.NotFoundResult(nameof(command.SubLedgerAccountId), "This subLedger account was not found.");

            if (!subLedgerAccountEntity.IsActive)
                return OperationResult<bool>.FailureResult(nameof(command.SubLedgerAccountId), "This subLedger account is not active.");

            if (subLedgerAccountEntity.JournalEntryType != command.JournalEntryType)
                return OperationResult<bool>.FailureResult(nameof(command.JournalEntryType), "Journal entry type does not match subLedger account.");

            // Check Detailed account
            if (command.DetailedAccountId is not null)
            {
                var detailedAccountEntity = await unitOfWork.DetailedAccountRepository.GetByIdAsync(command.DetailedAccountId.Value, cancellationToken);
                if (detailedAccountEntity is null)
                    return OperationResult<bool>.NotFoundResult(nameof(command.DetailedAccountId), "This detailed account was not found.");

                if (!detailedAccountEntity.IsActive)
                    return OperationResult<bool>.FailureResult(nameof(command.DetailedAccountId), "This detailed account is not active.");

                if (detailedAccountEntity.JournalEntryType != command.JournalEntryType)
                    return OperationResult<bool>.FailureResult(nameof(command.JournalEntryType), "Journal entry type does not match detailed account.");
            }

            lastEntryNum++;

            var journalEntryEntity = new JournalEntryEntity
            {
                JournalEntryNumber = lastEntryNum.ToString(),
                JournalEntryType = command.JournalEntryType,
                Amount = command.Amount,
                Description = command.Description,
                DetailedAccountId = command.DetailedAccountId,
                SubLedgerAccountId = command.SubLedgerAccountId,
                JournalVoucherId = command.JournalVoucherId
            };

            await unitOfWork.JournalEntryRepository.CreateAsync(journalEntryEntity, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}
