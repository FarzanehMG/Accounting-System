using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class UpdateSubLedgerAccountCommandHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<bool>> Handle(UpdateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSubLegerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Retrieve the existing sub ledger account
        var subLegerAccount = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(request.SubLegerAccountId, cancellationToken);

        if (subLegerAccount is null)
            return OperationResult<bool>.NotFoundResult(nameof(UpdateSubLedgerAccountCommand), "SubLeger account not found.");

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Title?.Value))
        {
            if (subLegerAccount.IsActive && request.Title.Value == subLegerAccount.AccountTitle)
            {
                return OperationResult<bool>.FailureResult(nameof(request.Title), "Active subLeger account exist."
                );
            }

            subLegerAccount.AccountTitle = request.Title.Value;
        }

        if (request.Description?.Value != null)
            subLegerAccount.AccountDescription = request.Description.Value;

        if (request.IsActive?.Value != null && request.IsActive.Value != subLegerAccount.IsActive)
        {
            subLegerAccount.IsActive = request.IsActive.Value;
        }

        // Business Rule: Ensure AccountCode is unique and follows the subLedger structure
        if (!string.IsNullOrWhiteSpace(request.Code?.Value) && request.Code?.Value != null && request.Code.Value != subLegerAccount.AccountCode)
        {
            var duplicateCode = await unitOfWork.SubLedgerAccountRepository
                .FindByCodeAsync(request.Code.Value, cancellationToken);

            if (duplicateCode != null)
            {
                return OperationResult<bool>.FailureResult(
                    nameof(request.Code),
                    "Account code already exists. Please provide a unique code.");
            }
            subLegerAccount.AccountCode = request.Code.Value;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}