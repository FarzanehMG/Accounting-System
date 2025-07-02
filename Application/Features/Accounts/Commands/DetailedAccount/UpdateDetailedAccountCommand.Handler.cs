using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class UpdateDetailedAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateDetailedAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateDetailedAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetByIdAsync(request.DetailedAccountId, cancellationToken);

        if (detailedAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateDetailedAccountCommand.DetailedAccountId), "Detailed Account Not Found.");

        if (request.Title?.Value != null)
            detailedAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            detailedAccount.AccountDescription = request.Description.Value;

        if (request.JournalEntryType?.Value != null)
            detailedAccount.JournalEntryType = request.JournalEntryType.Value;

        if (request.SubLedgerAccountId?.Value != null)
            detailedAccount.SubLedgerAccountId = request.SubLedgerAccountId.Value;

        if (request.IsActive != null && request.IsActive?.Value != detailedAccount.IsActive)
            if (request.IsActive?.Value != null)
                detailedAccount.IsActive = request.IsActive.Value;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}