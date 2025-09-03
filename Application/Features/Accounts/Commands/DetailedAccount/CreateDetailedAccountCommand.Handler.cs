using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class CreateDetailedAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateDetailedAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateDetailedAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Business Rule: A Detailed account must always inherit the same nature (JournalEntryType) as its Sub Ledger account.
        // This ensures accurate financial reporting, prevents mismatched balances, and aligns with standard accounting practices.
        var subLedgerAccount = await unitOfWork.SubLedgerAccountRepository
            .GetByIdAsync(request.SubLedgerAccountId, cancellationToken);

        if (subLedgerAccount == null)
            return OperationResult<bool>.NotFoundResult(nameof(request.SubLedgerAccountId), "SubLedger not found.");

        if (subLedgerAccount.JournalEntryType != request.JournalEntryType)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.JournalEntryType),
                    "Detailed account must have the same nature (JournalEntryType) as its SubLedger account.")]);
        }

        if (!subLedgerAccount.IsActive)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(nameof(request.SubLedgerAccountId),
                "The Sub Ledger account must be active.")]);
        }

        // Business Rule: Each active account within the same subLedger must have a unique title.
        // Prevents confusion in reporting and ensures accurate posting of journal entries.
        var existingAccounts = await unitOfWork.DetailedAccountRepository
            .FindAccountByTitleAndSubLedgerAndTypeAsync(request.Title, request.SubLedgerAccountId, request.JournalEntryType, cancellationToken);

        if (request.IsActive && existingAccounts.Any(x => x.IsActive))
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.Title), "An active account with this title already exists in the selected subLedger.")]);
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
        var duplicateCode = await unitOfWork.DetailedAccountRepository
            .FindByCodeAsync(request.Code, cancellationToken);

        if (duplicateCode is not null)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(
                "Code", "Account code already exists.")]);
        }

        // Create new detailed account entity
        var createDetailedAccount = new DetailedAccountEntity
        {
            AccountTitle = request.Title,
            AccountDescription = request.Description,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType,
            SubLedgerAccountId = request.SubLedgerAccountId,
            AccountCode = request.Code
        };

        await unitOfWork.DetailedAccountRepository.CreateAsync(createDetailedAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}