using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

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

        if (request.Title?.Value != null)
            groupAccount.AccountTitle = request.Title.Value;

        if (request.Description?.Value != null)
            groupAccount.AccountDescription = request.Description.Value;

        if (request.JournalEntryType?.Value != null)
            groupAccount.JournalEntryType = request.JournalEntryType.Value;

        if (request.IsActive != null && request.IsActive?.Value != groupAccount.IsActive)
            if (request.IsActive?.Value != null)
                groupAccount.IsActive = request.IsActive.Value;

        if (request.Code?.Value != null)
            groupAccount.AccountCode = request.Code.Value;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}