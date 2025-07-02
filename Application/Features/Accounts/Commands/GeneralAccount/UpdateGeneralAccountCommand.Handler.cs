using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public class UpdateGeneralAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateGeneralAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateGeneralAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalAccount = await unitOfWork.GeneralLegerAccountRepository.GetByIdAsync(request.GeneralAccountId, cancellationToken);

        if (generalAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateGeneralAccountCommand.GeneralAccountId), "General Account Not Found.");

        if (request.Title?.Value != null)
            generalAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            generalAccount.AccountDescription = request.Description.Value;

        if (request.JournalEntryType?.Value != null)
            generalAccount.JournalEntryType = request.JournalEntryType.Value;

        if (request.GroupAccountId?.Value != null)
            generalAccount.GroupAccountId= request.GroupAccountId.Value;

        if (request.IsActive != null && request.IsActive?.Value != generalAccount.IsActive)
            if (request.IsActive?.Value != null)
                generalAccount.IsActive = request.IsActive.Value;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}