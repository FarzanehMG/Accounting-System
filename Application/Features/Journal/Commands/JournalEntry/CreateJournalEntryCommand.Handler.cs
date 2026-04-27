using Application.Common;
using Application.Contracts.FileService.Interfaces;
using Application.Contracts.FileService.Models;
using Application.Contracts.JournalEntryService;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Application.Features.Journal.Commands.JournalEntry;

public class CreateJournalEntryCommandHandler(IUnitOfWork unitOfWork, IFileService fileService, IJournalEntryNumberGenerator numberGenerator)
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
            return OperationResult<bool>.FailureResult("Balance", "Journal entries are not balanced.");

        // Preload Required Data
        var subLedgerIds = commands.Select(c => c.SubLedgerAccountId).Distinct().ToList();

        var subLedgers = await unitOfWork.SubLedgerAccountRepository
            .GetByIdsAsync(subLedgerIds, cancellationToken);

        var subLedgerDict = subLedgers.ToDictionary(x => x.Id);

        var detailedIds = commands
            .Where(c => c.DetailedAccountId.HasValue)
            .Select(c => c.DetailedAccountId!.Value)
            .Distinct()
            .ToList();

        var detailedAccounts = detailedIds.Any()
            ? await unitOfWork.DetailedAccountRepository.GetByIdsAsync(detailedIds, cancellationToken)
            : new List<DetailedAccountEntity>();

        var detailedDict = detailedAccounts.ToDictionary(x => x.Id);

        // Command Validator
        var validator = new CreateJournalEntryCommandValidator();

        // Store entities for post-commit operations
        var createdEntries = new List<(JournalEntryEntity Entity, List<SaveFileModel>? Files)>();

        // Transactional Execution
        var result = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            foreach (var command in commands)
            {
                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                    throw new InvalidOperationException(validationResult.Errors.ConvertToKeyValuePair().First().Value);

                // SubLedger account validation
                if (!subLedgerDict.TryGetValue(command.SubLedgerAccountId, out var subLedger))
                    throw new InvalidOperationException("Invalid sub-ledger account.");

                if (!subLedger.IsActive)
                    throw new InvalidOperationException("SubLedger account is inactive.");

                if (subLedger.JournalEntryType != command.JournalEntryType)
                    throw new InvalidOperationException("Journal entry type mismatch.");

                // Detailed account validation
                if (command.DetailedAccountId.HasValue)
                {
                    if (!detailedDict.TryGetValue(command.DetailedAccountId.Value, out var detailed))
                        throw new InvalidOperationException("Invalid detailed account.");

                    if (!detailed.IsActive)
                        throw new InvalidOperationException("Detailed account is inactive.");

                    if (detailed.JournalEntryType != command.JournalEntryType)
                        throw new InvalidOperationException("Detailed account type mismatch.");
                }

                // Generate number
                var entryNumber = await numberGenerator.GenerateAsync(voucherId, cancellationToken);

                // Create journal entry
                var entity = new JournalEntryEntity
                {
                    JournalEntryNumber = entryNumber,
                    JournalEntryType = command.JournalEntryType,
                    Amount = command.Amount,
                    Description = command.Description,
                    DetailedAccountId = command.DetailedAccountId,
                    SubLedgerAccountId = command.SubLedgerAccountId,
                    JournalVoucherId = voucherId
                };

                await unitOfWork.JournalEntryRepository.CreateAsync(entity, cancellationToken);

                createdEntries.Add((entity, command.Attachments));
            }

            return OperationResult<bool>.SuccessResult(true);

        }, cancellationToken);

        // Post-Commit Side Effects
        if (result.IsSuccess)
        {
            foreach (var (entity, files) in createdEntries)
            {
                if (files is not null && files.Any())
                    await fileService.SaveFilesAsync(files, entity.Id, cancellationToken);
            }
        }

        return result;
    }
}
