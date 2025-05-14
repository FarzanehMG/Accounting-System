using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class CreateGroupAccountCommandHandler(IUnitOfWork unitOfWork)
{
	public async Task<OperationResult<bool>> Handle(CreateGroupAccountCommand request, CancellationToken cancellationToken)
	{
		var validator = new CreateGroupAccountCommandValidator();
		var validateResult = await validator.ValidateAsync(request, cancellationToken);

		if (!validateResult.IsValid)
			return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

		var createGroupAccount = new Domain.Entities.GroupAccount
		{
			// ToDo: Implement account code
			AccountCode = "1",
			AccountTitle = request.Title,
			AccountDescription = request.Description,
		};

		await unitOfWork.GroupAccountRepository.CreateAsync(createGroupAccount, cancellationToken);
		await unitOfWork.CommitAsync(cancellationToken);

		return OperationResult<bool>.SuccessResult(true);
	}
}