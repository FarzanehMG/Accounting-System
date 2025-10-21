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
            return OperationResult<bool>.NotFoundResult(nameof(UpdateGroupAccountCommand.GroupAccountId),"Group Account Not Found.");

        // Check if this GroupAccount has been used
        var hasJournalEntries = await unitOfWork.JournalEntryRepository
            .AnyByGroupAccountIdAsync(groupAccount.Id, cancellationToken);

        UpdateJournalEntryType(groupAccount, request, hasJournalEntries);
        UpdateAccountCode(groupAccount, request, hasJournalEntries);

        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
            groupAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            groupAccount.AccountDescription = request.Description.Value;

        if (request.IsActive != null && request.IsActive?.Value != groupAccount.IsActive)
            if (request.IsActive?.Value != null)
                groupAccount.IsActive = request.IsActive.Value;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }

    private static void UpdateJournalEntryType(GroupAccountEntity groupAccount, UpdateGroupAccountCommand request, bool hasJournalEntries)
    {
        if (request.JournalEntryType?.Value == null) return;

        if (hasJournalEntries && request.JournalEntryType.Value != groupAccount.JournalEntryType)
            throw new InvalidOperationException("Cannot change Journal Entry Type for a used account.");

        groupAccount.JournalEntryType = request.JournalEntryType.Value;
    }

    private static void UpdateAccountCode(GroupAccountEntity groupAccount, UpdateGroupAccountCommand request, bool hasJournalEntries)
    {
        if (string.IsNullOrWhiteSpace(request.Code?.Value)) return;

        if (hasJournalEntries && request.Code.Value != groupAccount.AccountCode)
            throw new InvalidOperationException("Cannot change account code for a used account.");

        groupAccount.AccountCode = request.Code.Value;
    }
}