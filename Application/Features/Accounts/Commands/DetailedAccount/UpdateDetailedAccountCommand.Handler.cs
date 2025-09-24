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

        // Retrieve the existing detailed account
        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetByIdAsync(request.DetailedAccountId, cancellationToken);

        if (detailedAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateDetailedAccountCommand), "Detailed account not found.");

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
        {
            if (detailedAccount.IsActive && request.Title.Value == detailedAccount.AccountTitle)
            {
                return OperationResult<bool>.FailureResult(nameof(request.Title), "Active detailed account exist."
                );
            }

            detailedAccount.AccountTitle = request.Title.Value;
        }

        if (request.Description?.Value != null)
            detailedAccount.AccountDescription = request.Description.Value;

        if (request.IsActive?.Value != null && request.IsActive.Value != detailedAccount.IsActive)
        {
            detailedAccount.IsActive = request.IsActive.Value;
        }

        // Business Rule: Ensure AccountCode is unique and follows the detailed structure
        if (request.Code?.Value != null && request.Code.Value != detailedAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.DetailedAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null)
            {
                return OperationResult<bool>.FailureResult(
                    nameof(request.Code),
                    "Account code already exists. Please provide a unique code.");
            }
            detailedAccount.AccountCode = request.Code.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}