using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class CreateGeneralLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateGeneralLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateGeneralLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Business Rule: financial period should be active
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null)
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("FinancialPeriod", "No active financial period found.")
            ]);

        // Retrieve the existing group account
        var groupAccount = await unitOfWork.GroupAccountRepository
            .GetByIdAsync(request.GroupAccountId, cancellationToken);

        if (groupAccount == null)
            return OperationResult<bool>.NotFoundResult(nameof(request.GroupAccountId), "Group account not found.");

        // Business Rule: A GeneralLedger account must always inherit the same nature (JournalEntryType) as its Group account.
        // This ensures accurate financial reporting, prevents mismatched balances, and aligns with standard accounting practices.
        if (groupAccount.JournalEntryType != request.JournalEntryType)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(nameof(request.JournalEntryType),
                    "GeneralLedger account must have the same nature (JournalEntryType) as its group account.")]);
        }

        // Business Rule: Each active account within the same group must have a unique title.
        // Prevents confusion in reporting and ensures accurate posting of journal entries.
        var existingAccounts = await unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(request.Title, request.GroupAccountId, request.JournalEntryType, cancellationToken);

        if (request.IsActive && existingAccounts.Any(a => a.IsActive))
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("Title", "An active account with this title already exists in the selected group.")]);
        }

        // Business Rule: If an account with the same title already exists but is inactive,
        // return its ID and code so the system can provide a direct "Reactivate" option.
        var inactiveAccount = existingAccounts.FirstOrDefault(a => !a.IsActive);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>(
                    "Title", $"An inactive account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>(
                    "InactiveAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        // TODO: Update this check to validate uniqueness per Company once Company feature is implemented.

        // Business Rule: Account code must always be unique to guarantee data integrity.
        var duplicateCode = await unitOfWork.GeneralLedgerAccountRepository
            .FindByCodeAndGroupAccountIdAsync(request.Code, request.GroupAccountId, cancellationToken);

        if (duplicateCode is not null)
        {
            return OperationResult<bool>.FailureResult([new KeyValuePair<string, string>(
                "Code", "Account code already exists.")]);
        }

        // Create new general ledger account entity
        var createGeneralAccount = new GeneralLedgerAccountEntity
        {
            AccountTitle = request.Title,
            AccountDescription = request.Description,
            JournalEntryType = request.JournalEntryType,
            IsActive = request.IsActive,
            GroupAccountId = request.GroupAccountId,
            AccountCode = request.Code,
        };

        await unitOfWork.GeneralLedgerAccountRepository.CreateAsync(createGeneralAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}