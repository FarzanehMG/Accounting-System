using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class UpdateGeneralLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateGeneralLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateGeneralLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Retrieve the existing general ledger account
        var generalAccount = await unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(request.GeneralLedgerAccountId, cancellationToken);

        if (generalAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateGeneralLedgerAccountCommand.GeneralLedgerAccountId), "General Account Not Found.");

        // Business Rule: Prevent duplicate active titles within the same group
        if (!string.IsNullOrWhiteSpace(request.Title?.Value) || request.GroupAccountId?.Value != null)
        {
            var titleToCheck = request.Title?.Value ?? generalAccount.AccountTitle;
            var groupToCheck = request.GroupAccountId?.Value ?? generalAccount.GroupAccountId;

            var existingAccounts = await unitOfWork.GeneralLedgerAccountRepository
                .FindActiveAccountByTitleAndGroupAsync(titleToCheck, groupToCheck, cancellationToken);

            if (existingAccounts.Any(a => a.Id != generalAccount.Id))
            {
                return OperationResult<bool>.FailureResult(
                    nameof(request.Title), "An active account with this title already exists in the selected group. Consider renaming or reactivating the existing account.");
            }
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
            generalAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            generalAccount.AccountDescription = request.Description.Value;

        if (request.JournalEntryType?.Value != null)
            generalAccount.JournalEntryType = request.JournalEntryType.Value;

        if (request.GroupAccountId?.Value != null)
        {
            if (request.GroupAccountId.Value == Guid.Empty)
            {
                return OperationResult<bool>.FailureResult(
                    nameof(request.GroupAccountId),
                    "GroupAccountId cannot be empty."
                );
            }

            generalAccount.GroupAccountId = request.GroupAccountId.Value;
        }

        if (request.IsActive?.Value != null && request.IsActive.Value != generalAccount.IsActive)
            generalAccount.IsActive = request.IsActive.Value;

        // Business Rule: Ensure AccountCode is unique and follows the group structure
        if (request.Code?.Value != null && request.Code.Value != generalAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.GeneralLedgerAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null)
            {
                return OperationResult<bool>.FailureResult(
                    nameof(request.Code),
                    "Account code already exists. Please provide a unique code.");
            }
            generalAccount.AccountCode = request.Code.Value;
        }

        if (unitOfWork.HasChanges(generalAccount))
        {
            generalAccount.ModifiedDate = DateTime.UtcNow;
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return OperationResult<bool>.SuccessResult(true);
    }
}