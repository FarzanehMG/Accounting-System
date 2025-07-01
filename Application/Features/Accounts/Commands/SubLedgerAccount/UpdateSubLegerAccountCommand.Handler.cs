using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class UpdateSubLegerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handel(UpdateSubLegerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSubLegerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var subLegerAccount = await unitOfWork.SubLegerAccountRepository.GetByIdAsync(request.SubLegerAccountId, cancellationToken);

        if (subLegerAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateSubLegerAccountCommand), "SubLeger account not found.");

        if (request.Title?.Value != null)
            subLegerAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            subLegerAccount.AccountDescription = request.Description.Value;

        if (request.JournalEntryType?.Value != null)
            subLegerAccount.JournalEntryType = request.JournalEntryType.Value;

        if (request.IsActive != null && request.IsActive?.Value != subLegerAccount.IsActive)
            if (request.IsActive?.Value != null)
                subLegerAccount.IsActive = request.IsActive.Value;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}