using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities.Accounts;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class CreateGroupAccountCommandHandler(IUnitOfWork unitOfWork)
{
	public async Task<OperationResult<bool>> Handle(CreateGroupAccountCommand request, CancellationToken cancellationToken)
	{
		var validator = new CreateGroupAccountCommandValidator();
		var validateResult = await validator.ValidateAsync(request, cancellationToken);

		if (!validateResult.IsValid)
			return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Business Rule: financial period should be active
        var activePeriod = await unitOfWork.FinancialPeriodRepository.GetActiveFinancialPeriodAsync(cancellationToken);
        if (activePeriod is null)
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("FinancialPeriod", "No active financial period found.")
            ]);

        // Business Rule: Each active account must have a unique title.
        // Prevents confusion in reporting and ensures accurate posting of journal entries.
        var existingGroups = await unitOfWork.GroupAccountRepository
            .FindAccountByTitleAndTypeAsync(request.Title, request.JournalEntryType, cancellationToken);

        if (request.IsActive && existingGroups.Any(g => g.IsActive))
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("Title", "An active group account with this title already exists.")
            ]);
        }

        // Business Rule: If an account with the same title already exists but is inactive,
        // return its ID and code so the system can provide a direct "Reactivate" option.
        var inactiveGroup = existingGroups.FirstOrDefault(g => !g.IsActive);
        if (inactiveGroup is not null)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("Title", $"An inactive group account with this title already exists (Code: {inactiveGroup.AccountCode}). Consider reactivating it instead of creating a new one."),
                new KeyValuePair<string, string>("InactiveGroupAccountId", inactiveGroup.Id.ToString())
            ]);
        }

        // Business Rule: Account code must always be unique to guarantee data integrity.
        var duplicateCode = await unitOfWork.GroupAccountRepository
            .FindByCodeAsync(request.Code, cancellationToken);

        if (duplicateCode is not null)
        {
            return OperationResult<bool>.FailureResult([
                new KeyValuePair<string, string>("Code", "Group account code already exists.")
            ]);
        }

        // Create new group account entity
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