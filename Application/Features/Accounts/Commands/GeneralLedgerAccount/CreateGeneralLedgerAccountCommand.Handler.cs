using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class CreateGeneralAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(CreateGeneralLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateGeneralLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

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

        // Technical Rule: Account code must always be unique to guarantee data integrity.
        var duplicateCode = await unitOfWork.GeneralLedgerAccountRepository
            .FindByCodeAsync(request.Code, cancellationToken);

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