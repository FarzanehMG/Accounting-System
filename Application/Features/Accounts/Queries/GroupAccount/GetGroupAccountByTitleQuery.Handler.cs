using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountByTitleQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetGroupAccountByTitleQueryResult>> Handle(GetGroupAccountByTitleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGroupAccountByTitleQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetGroupAccountByTitleQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var groupAccount = await unitOfWork.GroupAccountRepository.GetByTitleAsync(request.AccountTitle, cancellationToken);

        if (groupAccount is null)
            return OperationResult<GetGroupAccountByTitleQueryResult>.NotFoundResult(nameof(GetGroupAccountByTitleQueryResult.AccountTitle), "Group account not found.");

        return OperationResult<GetGroupAccountByTitleQueryResult>.SuccessResult(new GetGroupAccountByTitleQueryResult(groupAccount.Id, groupAccount.AccountTitle));
    }
}