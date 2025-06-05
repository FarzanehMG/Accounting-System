using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountByTitleQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<List<GetGroupAccountByTitleQueryResult>>> Handle(GetGroupAccountByTitleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGroupAccountByTitleQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<List<GetGroupAccountByTitleQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var groupAccount = await unitOfWork.GroupAccountRepository.GetByTitleAsync(request.AccountTitle, cancellationToken);

        if (!groupAccount.Any())
            return OperationResult<List<GetGroupAccountByTitleQueryResult>>.NotFoundResult(nameof(GetGroupAccountByTitleQueryResult.AccountTitle), "Group account not found.");

        return OperationResult<List<GetGroupAccountByTitleQueryResult>>.SuccessResult(groupAccount.Select(x=> new GetGroupAccountByTitleQueryResult(x.Id, x.AccountTitle)).ToList());
    }
}