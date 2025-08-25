using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
	public async Task<OperationResult<GetGroupAccountByIdQueryResult>> Handle(GetGroupAccountByIdQuery request,CancellationToken cancellationToken)
	{
		var validator = new GetGroupAccountByIdQueryValidator();
		var validateResult = await validator.ValidateAsync(request, cancellationToken);

		if (!validateResult.IsValid)
			return OperationResult<GetGroupAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

		var groupAccount = await unitOfWork.GroupAccountRepository.GetByIdAsync(request.Id, cancellationToken);

		if (groupAccount is null)
			return OperationResult<GetGroupAccountByIdQueryResult>.NotFoundResult(nameof(GetGroupAccountByIdQuery.Id), "Group account not found.");
		
		return OperationResult<GetGroupAccountByIdQueryResult>.SuccessResult(new GetGroupAccountByIdQueryResult
        {
            Id = groupAccount.Id,
            AccountTitle = groupAccount.AccountTitle
        });
	}
}