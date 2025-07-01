using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class CreateSubLedgerAccountCommandHandler(IUnitOfWork unitOfWork, IAccountCodeGenerator codeGenerator)
{
    public async Task<OperationResult<bool>> Handel(CreateSubLedgerAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSubLedgerAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var createSubLegerAccount = new SubLedgerAccountEntity
        {
            AccountTitle = request.Title,
            AccountDescription = request.Description,
            JournalEntryType = request.JournalEntryType,
            IsActive = request.IsActive,
            GeneralLedgerAccountId = request.GeneralLegerAccountId,
            AccountCode = codeGenerator.GenerateCode(),
        };
        
        await unitOfWork.SubLegerAccountRepository.CreateAsync(createSubLegerAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}