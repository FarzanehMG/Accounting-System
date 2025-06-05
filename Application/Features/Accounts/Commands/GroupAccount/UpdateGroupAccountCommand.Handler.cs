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

        if (request.Title != null) 
            groupAccount.AccountTitle = request.Title;

        if (request.Description != null)
            groupAccount.AccountDescription = request.Description;

        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}