using Application.Common;
using Application.Contracts.FileService.Interfaces;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalEntry;

public class CreateJournalEntryCommandHandler(IUnitOfWork unitOfWork, IFileService fileService)
{
    public async Task<OperationResult<bool>> Handle(List<CreateJournalEntryCommand> commands, CancellationToken cancellationToken)
    {
        if (!commands.Any() || commands.Count < 2)
            return OperationResult<bool>.FailureResult("Entries", "At least two journal entry is required.");

        // Ensure all commands target the same JournalVoucherId
        var voucherId = commands.First().JournalVoucherId;
        if (commands.Any(c => c.JournalVoucherId != voucherId))
            return OperationResult<bool>.FailureResult("JournalVoucherId", "All entries must belong to the same journal voucher.");

        // Check Journal Voucher
        var journalVoucher = await unitOfWork.JournalVoucherRepository
            .GetByIdAsync(voucherId, cancellationToken);

        if (journalVoucher is null)
            return OperationResult<bool>.NotFoundResult(nameof(voucherId), "Journal voucher not found.");

        if (journalVoucher.JournalVoucherState != JournalVoucherState.Draft)
            return OperationResult<bool>.FailureResult(nameof(journalVoucher.JournalVoucherState), "Cannot add entries to a non-draft voucher.");

        // Business Rule: Entries must be balanced (Debit = Credit)
        var debitTotal = commands
            .Where(c => c.JournalEntryType == JournalEntryType.Debit)
            .Sum(c => c.Amount);

        var creditTotal = commands
            .Where(c => c.JournalEntryType == JournalEntryType.Credit)
            .Sum(c => c.Amount);

        if (debitTotal != creditTotal)
            return OperationResult<bool>.FailureResult("Entries", "Journal entries are not balanced.");

        // Generate Entry Number (incremental per JournalVoucher)
        var lastEntry = await unitOfWork.JournalEntryRepository.GetLastEntryByVoucherIdAsync(voucherId, cancellationToken);

        var lastEntryNum = lastEntry is null ? 0 : int.Parse(lastEntry.JournalEntryNumber);

        // Validator
        var validator = new CreateJournalEntryCommandValidator();

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var command in commands)
            {
                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException(validationResult.Errors.ConvertToKeyValuePair().First().Value);

                var subLedgerAccountEntity = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(command.SubLedgerAccountId, cancellationToken);

                if (subLedgerAccountEntity is null || !subLedgerAccountEntity.IsActive)
                    return OperationResult<bool>.FailureResult(nameof(command.SubLedgerAccountId), "Invalid or inactive sub-ledger account.");

                if (subLedgerAccountEntity.JournalEntryType != command.JournalEntryType)
                    return OperationResult<bool>.FailureResult(nameof(command.JournalEntryType), "Journal entry type mismatch.");

                // Check Detailed account
                if (command.DetailedAccountId is not null)
                {
                    var detailedAccountEntity = await unitOfWork.DetailedAccountRepository.GetByIdAsync(command.DetailedAccountId.Value, cancellationToken);

                    if (detailedAccountEntity is null || !detailedAccountEntity.IsActive)
                        return OperationResult<bool>.FailureResult(nameof(command.DetailedAccountId), "Invalid or inactive detailed account.");

                    if (detailedAccountEntity.JournalEntryType != command.JournalEntryType)
                        return OperationResult<bool>.FailureResult(nameof(command.JournalEntryType), "Journal entry type mismatch.");
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
                    JournalVoucherId = voucherId
                };

                await unitOfWork.JournalEntryRepository.CreateAsync(journalEntryEntity, cancellationToken);

                // Save files
                await fileService.SaveFilesAsync(command.Attachments, journalEntryEntity.Id, cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return OperationResult<bool>.FailureResult("Posting", ex.Message);
        }
    }
}
