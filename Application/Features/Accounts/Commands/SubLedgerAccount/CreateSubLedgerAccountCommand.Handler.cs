using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class CreateSubLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSubLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Business Rule: A Sub Ledger account must always inherit the same nature (JournalEntryType) as its General Ledger account.
        // This ensures accurate financial reporting, prevents mismatched balances, and aligns with standard accounting practices.

        var generalLedger = await unitOfWork.GeneralLedgerAccountRepository
            .GetByIdAsync(request.GeneralLedgerAccountId, cancellationToken);

        if (generalLedger == null)
            return OperationResult<bool>.NotFoundResult(nameof(request.GeneralLedgerAccountId), "General ledger not found.");

        if (generalLedger.JournalEntryType != request.JournalEntryType)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.JournalEntryType),
                    "Sub Ledger account must have the same nature (JournalEntryType) as its General Ledger account.")]);
        }

        if (!generalLedger.IsActive)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(nameof(request.GeneralLedgerAccountId),
                "The General Ledger account must be active.")]);
        }

        // Business Rule: Each active account within the same generalLedger must have a unique title.
        // Prevents confusion in reporting and ensures accurate posting of journal entries.
        var existingAccounts = await unitOfWork.SubLegerAccountRepository
            .FindAccountByTitleAndGeneralLedgerAndTypeAsync(request.Title, request.GeneralLedgerAccountId, request.JournalEntryType, cancellationToken);

        if (request.IsActive && existingAccounts.Any(x => x.IsActive))
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.Title), "An active account with this title already exists in the selected generalLedger.")]);
        }

        // Business Rule: If an account with the same title already exists but is inactive,
        // return its ID and code so the system can provide a direct "Reactivate" option.
        var inactiveAccount = existingAccounts.FirstOrDefault(x => !x.IsActive);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(
                    nameof(request.Title), $"An inactive account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>(
                    "InactiveAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        // Technical Rule: Account code must always be unique to guarantee data integrity.
        var duplicateCode = await unitOfWork.SubLegerAccountRepository
            .FindByCodeAsync(request.Code, cancellationToken);

        if (duplicateCode is not null)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(
                "Code", "Account code already exists.")]);
        }

        // Create new sub ledger account entity
        var createSubLegerAccount = new SubLedgerAccountEntity
        {
            AccountTitle = request.Title,
            AccountDescription = request.Description,
            JournalEntryType = request.JournalEntryType,
            IsActive = request.IsActive,
            GeneralLedgerAccountId = request.GeneralLedgerAccountId,
            AccountCode = request.Code
        };

        await unitOfWork.SubLegerAccountRepository.CreateAsync(createSubLegerAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}