using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class UpdateDetailedAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateDetailedAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateDetailedAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Ensure there is an active financial period
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null || !activePeriod.IsActive)
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Cannot modify detailed accounts without an active open financial period.");

        // Retrieve the existing detailed account
        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetByIdAsync(request.DetailedAccountId, cancellationToken);

        if (detailedAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateDetailedAccountCommand), "Detailed Account Not Found.");

        // Check if the account has journal entries
        var hasJournalEntries = await unitOfWork.JournalEntryRepository
            .AnyJournalEntryByGroupAccountIdAsync(detailedAccount.SubLedgerAccount!.GeneralLedgerAccount!.GroupAccountId, cancellationToken);

        // Update title with duplicate check
        var titleResult = await UpdateAccountTitle(detailedAccount, request, cancellationToken);
        if (!titleResult.IsSuccess)
            return titleResult;

        // Update account code
        var codeResult = await UpdateAccountCode(detailedAccount, request, hasJournalEntries, cancellationToken);
        if (!codeResult.IsSuccess)
            return codeResult;

        // Update journal type
        var journalTypeResult = UpdateJournalEntryType(detailedAccount, request, hasJournalEntries);
        if (!journalTypeResult.IsSuccess)
            return journalTypeResult;

        // Update subLedger account
        var subLedgerAccountResult = await UpdateSubLedgerAccountId(detailedAccount, request, hasJournalEntries, cancellationToken);
        if (!subLedgerAccountResult.IsSuccess)
            return subLedgerAccountResult;

        // Update description
        if (!string.IsNullOrWhiteSpace(request.Description?.Value))
            detailedAccount.AccountDescription = request.Description.Value;

        // Update IsActive
        if (request.IsActive?.Value != null && request.IsActive.Value != detailedAccount.IsActive)
        {
            if (hasJournalEntries && !request.IsActive.Value)
                return OperationResult<bool>.FailureResult(nameof(request.IsActive), "Cannot deactivate an account with existing journal entries.");

            detailedAccount.IsActive = request.IsActive.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountTitle(DetailedAccountEntity detailedAccount, UpdateDetailedAccountCommand request, CancellationToken cancellationToken)
    {
        if (request.Title is null)
            return OperationResult<bool>.SuccessResult(true);

        if (string.IsNullOrWhiteSpace(request.Title.Value))
            return OperationResult<bool>.FailureResult(nameof(request.Title), "Title cannot be empty.");

        var titleToCheck = request.Title?.Value ?? detailedAccount.AccountTitle;
        var subLedgerToCheck = request.SubLedgerAccountId?.Value ?? detailedAccount.SubLedgerAccountId;

        var existingAccounts = await unitOfWork.DetailedAccountRepository
            .FindAccountByTitleAndSubLedgerAccountAsync(titleToCheck, subLedgerToCheck, cancellationToken);

        if (request.IsActive?.Value != false && existingAccounts.Any(a => a.IsActive && a.Id != detailedAccount.Id))
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", "An active account with this title already exists in the selected detailed account.")
            ]);
        }

        var inactiveAccount = existingAccounts.FirstOrDefault(a => !a.IsActive && a.Id != detailedAccount.Id);
        if (inactiveAccount is not null)
        {
            return OperationResult<bool>.FailureResult([

                new KeyValuePair<string, string>("Title", $"An inactive account with this title already exists (Code: {inactiveAccount.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>("InactiveAccountId", inactiveAccount.Id.ToString())
            ]);
        }

        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
            detailedAccount.AccountTitle = request.Title.Value;

        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateAccountCode(DetailedAccountEntity detailedAccount, UpdateDetailedAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (request.Code is null)
            return OperationResult<bool>.SuccessResult(true);

        if (string.IsNullOrWhiteSpace(request.Code.Value))
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Code cannot be empty.");

        if (hasJournalEntries && request.Code.Value != detailedAccount.AccountCode)
            return OperationResult<bool>.FailureResult(nameof(request.Code), "Cannot change account code for an account with journal entries.");

        if (request.Code.Value != detailedAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.DetailedAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null && duplicateCode.Id != detailedAccount.Id)
                return OperationResult<bool>.FailureResult(nameof(request.Code), "Account code already exists. Please provide a unique code.");

            detailedAccount.AccountCode = request.Code.Value;
        }

        return OperationResult<bool>.SuccessResult(true);
    }

    private static OperationResult<bool> UpdateJournalEntryType(DetailedAccountEntity detailedAccount, UpdateDetailedAccountCommand request, bool hasJournalEntries)
    {
        if (request.JournalEntryType?.Value == null)
            return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.JournalEntryType.Value != detailedAccount.JournalEntryType)
            return OperationResult<bool>.FailureResult(nameof(request.JournalEntryType), "Cannot change Journal Entry Type for an account with existing journal entries.");

        detailedAccount.JournalEntryType = request.JournalEntryType.Value;
        return OperationResult<bool>.SuccessResult(true);
    }

    private async Task<OperationResult<bool>> UpdateSubLedgerAccountId(DetailedAccountEntity detailedAccount, UpdateDetailedAccountCommand request, bool hasJournalEntries, CancellationToken cancellationToken)
    {
        if (request.SubLedgerAccountId?.Value is null)
            return OperationResult<bool>.SuccessResult(true);

        if (hasJournalEntries && request.SubLedgerAccountId.Value != detailedAccount.SubLedgerAccountId)
            return OperationResult<bool>.FailureResult(nameof(request.SubLedgerAccountId), "Cannot change SubLedger Account ID for a subLedger that already has journal entries.");

        if (request.SubLedgerAccountId.Value == Guid.Empty)
            return OperationResult<bool>.FailureResult(nameof(request.SubLedgerAccountId), "SubLedgerAccountId cannot be empty.");

        var generalLedgerAccount = await unitOfWork.SubLedgerAccountRepository
            .GetByIdAsync(request.SubLedgerAccountId.Value, cancellationToken);

        if (generalLedgerAccount is null || !generalLedgerAccount.IsActive)
            return OperationResult<bool>.FailureResult(nameof(request.SubLedgerAccountId), "Cannot assign account to an inactive or non-existent subLedger account.");

        detailedAccount.SubLedgerAccountId = request.SubLedgerAccountId.Value;

        return OperationResult<bool>.SuccessResult(true);
    }
}