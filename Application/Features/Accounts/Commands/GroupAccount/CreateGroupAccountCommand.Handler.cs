using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class CreateGroupAccountCommandHandler(IUnitOfWork unitOfWork)
{
	public async Task<OperationResult<bool>> Handle(CreateGroupAccountCommand request, CancellationToken cancellationToken)
	{
		var validator = new CreateGroupAccountCommandValidator();
		var validateResult = await validator.ValidateAsync(request, cancellationToken);

		if (!validateResult.IsValid)
			return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        if (request.IsActive)
        {
            var exists = await unitOfWork.GroupAccountRepository
                .FindActiveAccountByTitleAndTypeAsync(request.Title, request.JournalEntryType, cancellationToken);

            if (exists != null)
            {
                return OperationResult<bool>.FailureResult(nameof(request.Title), "An active group account with the same title or type already exists.");
            }
        }

        var createGroupAccount = new GroupAccountEntity
		{
			AccountTitle = request.Title,
			AccountDescription = request.Description,
			JournalEntryType = request.JournalEntryType,
			IsActive = request.IsActive,
            AccountCode = request.Code,
        };

		await unitOfWork.GroupAccountRepository.CreateAsync(createGroupAccount, cancellationToken);
		await unitOfWork.CommitAsync(cancellationToken);

		return OperationResult<bool>.SuccessResult(true);
	}
}