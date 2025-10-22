using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

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

        // Ensure there is an active financial period
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null || activePeriod.IsActive)
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Cannot modify accounts without an active open financial period.");

        // Check if the account has journal entries
        var hasJournalEntries = await unitOfWork.JournalEntryRepository
            .AnyJournalEntryByGroupAccountIdAsync(generalAccount.Id, cancellationToken);

        // Update title with duplicate check
        var titleResult = await UpdateAccountTitle(generalAccount, request, cancellationToken);
        if (!titleResult.IsSuccess) return titleResult;

        // Update account code
        var codeResult = await UpdateAccountCode(generalAccount, request, hasJournalEntries, cancellationToken);
        if (!codeResult.IsSuccess) return codeResult;

        // Update journal type
        var journalTypeResult = UpdateJournalEntryType(generalAccount, request, hasJournalEntries);
        if (!journalTypeResult.IsSuccess) return journalTypeResult;

        // Update group account if provided
        if (request.GroupAccountId?.Value != null)
        {
            if (request.GroupAccountId.Value == Guid.Empty)
                return OperationResult<bool>.FailureResult(nameof(request.GroupAccountId), "GroupAccountId cannot be empty.");

            var group = await unitOfWork.GroupAccountRepository.GetByIdAsync(request.GroupAccountId.Value, cancellationToken);
            if (group is null || !group.IsActive)
                return OperationResult<bool>.FailureResult(nameof(request.GroupAccountId), "Cannot assign account to an inactive or non-existent group.");

            generalAccount.GroupAccountId = request.GroupAccountId.Value;
        }

        // Update description
        if (!string.IsNullOrWhiteSpace(request.Description?.Value))
            generalAccount.AccountDescription = request.Description.Value;

        // Update IsActive
        if (request.IsActive?.Value != null && request.IsActive.Value != generalAccount.IsActive)
        {
            if (hasJournalEntries && !request.IsActive.Value)
                return OperationResult<bool>.FailureResult(nameof(request.IsActive), "Cannot deactivate an account with existing journal entries.");

            generalAccount.IsActive = request.IsActive.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }

    private static OperationResult<bool> UpdateJournalEntryType(GeneralLedgerAccountEntity generalLedgerAccount, UpdateGeneralLedgerAccountCommand request, bool hasJournalEntries)
    {
        if (request.JournalEntryType?.Value == null) return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.JournalEntryType.Value != generalLedgerAccount.JournalEntryType)
            return OperationResult<bool>.FailureResult(nameof(request.JournalEntryType), "Cannot change Journal Entry Type for an account with existing journal entries.");

        generalLedgerAccount.JournalEntryType = request.JournalEntryType.Value;
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountCode(GeneralLedgerAccountEntity generalLedgerAccount, UpdateGeneralLedgerAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code?.Value))
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Account code cannot be empty.");

        if (hasJournalEntries && request.Code.Value != generalLedgerAccount.AccountCode)
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Cannot change account code for an account with journal entries.");

        if (request.Code.Value != generalLedgerAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.GeneralLedgerAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null && duplicateCode.Id != generalLedgerAccount.Id)
                return OperationResult<bool>.FailureResult(nameof(request.Code), "Account code already exists. Please provide a unique code.");

            generalLedgerAccount.AccountCode = request.Code.Value;
        }

        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountTitle(GeneralLedgerAccountEntity generalLedgerAccount, UpdateGeneralLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title?.Value) && request.GroupAccountId?.Value == null)
            return OperationResult<bool>.SuccessResult(true);

        var titleToCheck = request.Title?.Value ?? generalLedgerAccount.AccountTitle;
        var groupToCheck = request.GroupAccountId?.Value ?? generalLedgerAccount.GroupAccountId;

        var existingAccounts = await unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAccountAsync(titleToCheck, groupToCheck, cancellationToken);

        if (request.IsActive?.Value != false && existingAccounts.Any(a => a.IsActive && a.Id != generalLedgerAccount.Id))
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", "An active account with this title already exists in the selected group.")
                ]);
        }

        var inactiveAccount = existingAccounts.FirstOrDefault(a => !a.IsActive && a.Id != generalLedgerAccount.Id);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", $"An inactive account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>("InactiveAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
            generalLedgerAccount.AccountTitle = request.Title.Value;

        return OperationResult<bool>.SuccessResult(true);
    }
}