using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class CreateSubLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSubLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Business Rule: financial period should be active
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null)
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("FinancialPeriod", "No active financial period found.")
            ]);

        // Retrieve the existing generalLedger account
        var generalLedger = await unitOfWork.GeneralLedgerAccountRepository
            .GetByIdAsync(request.GeneralLedgerAccountId, cancellationToken);

        if (generalLedger == null)
            return OperationResult<bool>.NotFoundResult(nameof(request.GeneralLedgerAccountId), "GeneralLedger not found.");

        // Business Rule: A Sub Ledger account must always inherit the same nature (JournalEntryType) as its General Ledger account.
        // This ensures accurate financial reporting, prevents mismatched balances, and aligns with standard accounting practices.
        if (generalLedger.JournalEntryType != request.JournalEntryType)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.JournalEntryType),
                    "SubLedger account must have the same nature (JournalEntryType) as its GeneralLedger account.")]);
        }

        if (!generalLedger.IsActive)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(nameof(request.GeneralLedgerAccountId),
                "The GeneralLedger account must be active.")]);
        }

        // Business Rule: Each active account within the same generalLedger must have a unique title.
        // Prevents confusion in reporting and ensures accurate posting of journal entries.
        var existingAccounts = await unitOfWork.SubLedgerAccountRepository
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

        // TODO: Update this check to validate uniqueness per Company once Company feature is implemented.

        // Business Rule: Account code must always be unique to guarantee data integrity.
        var duplicateCode = await unitOfWork.SubLedgerAccountRepository
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

        await unitOfWork.SubLedgerAccountRepository.CreateAsync(createSubLegerAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}