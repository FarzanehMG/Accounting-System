using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class UpdateGroupAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateGroupAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateGroupAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var groupAccount = await unitOfWork.GroupAccountRepository.GetByIdAsync(request.GroupAccountId, cancellationToken);
        if (groupAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateGroupAccountCommand.GroupAccountId), "Group Account Not Found.");

        // Ensure there is an active financial period
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null || !activePeriod.IsActive)
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Cannot modify group accounts without an active open financial period.");

        // Check if this group account has journal entries
        var hasJournalEntries = await unitOfWork.JournalEntryRepository
            .AnyJournalEntryByGroupAccountIdAsync(groupAccount.Id, cancellationToken);

        // Update journal entry type
        var journalTypeResult = UpdateJournalEntryType(groupAccount, request, hasJournalEntries);
        if (!journalTypeResult.IsSuccess) return journalTypeResult;

        // Update account code
        var codeResult = await UpdateAccountCode(groupAccount, request, hasJournalEntries, cancellationToken);
        if (!codeResult.IsSuccess) return codeResult;

        // Prevent duplicate active titles and handle inactive duplicates
        var titleResult = await UpdateAccountTitle(groupAccount, request, cancellationToken);
        if (!titleResult.IsSuccess) return titleResult;

        // Update description
        if (!string.IsNullOrWhiteSpace(request.Description?.Value))
            groupAccount.AccountDescription = request.Description.Value;

        // Update IsActive
        if (request.IsActive?.Value != null && request.IsActive.Value != groupAccount.IsActive)
        {
            if (hasJournalEntries && !request.IsActive.Value)
                return OperationResult<bool>.FailureResult(nameof(request.IsActive), "Cannot deactivate a group account with existing journal entries.");

            groupAccount.IsActive = request.IsActive.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }

    private static OperationResult<bool> UpdateJournalEntryType(GroupAccountEntity groupAccount, UpdateGroupAccountCommand request, bool hasJournalEntries)
    {
        if (request.JournalEntryType?.Value == null) return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.JournalEntryType.Value != groupAccount.JournalEntryType)
            return OperationResult<bool>.FailureResult(nameof(request.JournalEntryType), "Cannot change Journal Entry Type for a used account.");

        groupAccount.JournalEntryType = request.JournalEntryType.Value;
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountCode(GroupAccountEntity groupAccount, UpdateGroupAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code?.Value))
            return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.Code.Value != groupAccount.AccountCode)
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Cannot change account code for a used account.");

        if (request.Code.Value != groupAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.GroupAccountRepository.FindByCodeAsync(request.Code.Value, cancellationToken);
            if (duplicateCode != null && duplicateCode.Id != groupAccount.Id)
                return OperationResult<bool>.FailureResult(nameof(request.Code), "Group account code already exists. Please provide a unique code.");

            groupAccount.AccountCode = request.Code.Value;
        }

        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountTitle(GroupAccountEntity groupAccount, UpdateGroupAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title?.Value)) return OperationResult<bool>.SuccessResult(true);

        var titleToCheck = request.Title.Value;
        var journalEntryTypeToCheck = request.JournalEntryType?.Value ?? groupAccount.JournalEntryType;

        var existingAccounts = await unitOfWork.GroupAccountRepository
            .FindAccountByTitleAndTypeAsync(titleToCheck, journalEntryTypeToCheck, cancellationToken);

        // Check for active duplicates
        if (existingAccounts.Any(a => a.IsActive && a.Id != groupAccount.Id))
        {
            return OperationResult<bool>.FailureResult(nameof(request.Title),
                "An active group account with this title already exists. Consider renaming or reactivating the existing account.");
        }

        // Check for inactive duplicates
        var inactiveAccount = existingAccounts.FirstOrDefault(a => !a.IsActive && a.Id != groupAccount.Id);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([
            
                new KeyValuePair<string,string>("Title", $"An inactive group account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string,string>("InactiveGroupAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        groupAccount.AccountTitle = titleToCheck;
        return OperationResult<bool>.SuccessResult(true);
    }
}