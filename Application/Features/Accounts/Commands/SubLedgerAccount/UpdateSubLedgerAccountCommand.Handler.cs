using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class UpdateSubLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSubLegerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Ensure there is an active financial period
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null || !activePeriod.IsActive)
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Cannot modify subLedger accounts without an active open financial period.");

        // Retrieve the existing subLedger account
        var subLedgerAccount = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(request.SubLedgerAccountId, cancellationToken);

        if (subLedgerAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateSubLedgerAccountCommand), "SubLedger Account Not Found.");

        // Check if the account has journal entries
        var hasJournalEntries = subLedgerAccount.GeneralLedgerAccount != null && await unitOfWork.JournalEntryRepository
            .AnyJournalEntryByGroupAccountIdAsync(subLedgerAccount.GeneralLedgerAccount.GroupAccountId, cancellationToken);

        // Update title with duplicate check
        var titleResult = await UpdateAccountTitle(subLedgerAccount, request, cancellationToken);
        if (!titleResult.IsSuccess)
            return titleResult;

        // Update account code
        var codeResult = await UpdateAccountCode(subLedgerAccount, request, hasJournalEntries, cancellationToken);
        if (!codeResult.IsSuccess)
            return codeResult;

        // Update journal type
        var journalTypeResult = UpdateJournalEntryType(subLedgerAccount, request, hasJournalEntries);
        if (!journalTypeResult.IsSuccess)
            return journalTypeResult;

        // Update generalLedger account
        var generalLedgerAccountResult = await UpdateGeneralLedgerAccountId(subLedgerAccount, request, hasJournalEntries, cancellationToken);
        if (!generalLedgerAccountResult.IsSuccess)
            return generalLedgerAccountResult;

        // Update description
        if (!string.IsNullOrWhiteSpace(request.Description?.Value))
            subLedgerAccount.AccountDescription = request.Description.Value;

        // Update IsActive
        if (request.IsActive?.Value != null && request.IsActive.Value != subLedgerAccount.IsActive)
        {
            if (hasJournalEntries && !request.IsActive.Value)
                return OperationResult<bool>.FailureResult(nameof(request.IsActive), "Cannot deactivate an account with existing journal entries.");

            subLedgerAccount.IsActive = request.IsActive.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountTitle(SubLedgerAccountEntity subLedgerAccount, UpdateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        if (request.Title is null)
            return OperationResult<bool>.SuccessResult(true);

        if (string.IsNullOrWhiteSpace(request.Title.Value))
            return OperationResult<bool>.FailureResult(nameof(request.Title), "Title cannot be empty.");

        var titleToCheck = request.Title?.Value ?? subLedgerAccount.AccountTitle;
        var generalLedgerToCheck = request.GeneralLedgerAccountId?.Value ?? subLedgerAccount.GeneralLedgerAccountId;

        var existingAccounts = await unitOfWork.SubLedgerAccountRepository
            .FindAccountByTitleAndGeneralLedgerAccountAsync(titleToCheck, generalLedgerToCheck, cancellationToken);

        if (request.IsActive?.Value != false && existingAccounts.Any(a => a.IsActive && a.Id != subLedgerAccount.Id))
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", "An active account with this title already exists in the selected generalLedger account.")
            ]);
        }

        var inactiveAccount = existingAccounts.FirstOrDefault(a => !a.IsActive && a.Id != subLedgerAccount.Id);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", $"An inactive account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>("InactiveAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
            subLedgerAccount.AccountTitle = request.Title.Value;

        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountCode(SubLedgerAccountEntity subLedgerAccount, UpdateSubLedgerAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (request.Code is null)
            return OperationResult<bool>.SuccessResult(true);

        if (string.IsNullOrWhiteSpace(request.Code.Value))
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Code cannot be empty.");

        if (hasJournalEntries && request.Code.Value != subLedgerAccount.AccountCode)
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Cannot change account code for an account with journal entries.");

        if (request.Code.Value != subLedgerAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.SubLedgerAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null && duplicateCode.Id != subLedgerAccount.Id)
                return OperationResult<bool>.FailureResult(nameof(request.Code), "Account code already exists. Please provide a unique code.");

            subLedgerAccount.AccountCode = request.Code.Value;
        }

        return OperationResult<bool>.SuccessResult(true);
    }

    private static OperationResult<bool> UpdateJournalEntryType(SubLedgerAccountEntity subLedgerAccount, UpdateSubLedgerAccountCommand request, bool hasJournalEntries)
    {
        if (request.JournalEntryType?.Value == null)
            return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.JournalEntryType.Value != subLedgerAccount.JournalEntryType)
            return OperationResult<bool>.FailureResult(nameof(request.JournalEntryType), "Cannot change Journal Entry Type for an account with existing journal entries.");

        subLedgerAccount.JournalEntryType = request.JournalEntryType.Value;
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateGeneralLedgerAccountId(SubLedgerAccountEntity subLedgerAccount, UpdateSubLedgerAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (request.GeneralLedgerAccountId?.Value is null)
            return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.GeneralLedgerAccountId.Value != subLedgerAccount.GeneralLedgerAccountId)
            return OperationResult<bool>.FailureResult(nameof(request.GeneralLedgerAccountId), "Cannot change General Ledger Account ID for a subLedger that already has journal entries.");

        if (request.GeneralLedgerAccountId.Value == Guid.Empty)
            return OperationResult<bool>.FailureResult(nameof(request.GeneralLedgerAccountId), "GeneralLedgerAccountId cannot be empty.");

        var generalLedgerAccount = await unitOfWork.GeneralLedgerAccountRepository
            .GetByIdAsync(request.GeneralLedgerAccountId.Value, cancellationToken);

        if (generalLedgerAccount is null || !generalLedgerAccount.IsActive)
            return OperationResult<bool>.FailureResult(nameof(request.GeneralLedgerAccountId), "Cannot assign account to an inactive or non-existent generalLedger account.");

        subLedgerAccount.GeneralLedgerAccountId = request.GeneralLedgerAccountId.Value;

        return OperationResult<bool>.SuccessResult(true);
    }

}