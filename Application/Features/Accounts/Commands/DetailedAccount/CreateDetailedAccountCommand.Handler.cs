using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class CreateDetailedAccountCommandHandler(IUnitOfWork unitOfWork, IAccountCodeGenerator codeGenerator)
{
    public async Task<OperationResult<bool>> Handle(CreateDetailedAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateDetailedAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var createDetailedAccount = new DetailedAccountEntity
        {
            AccountTitle = request.Title,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType,
            SubLedgerAccountId = request.SubLedgerAccountId,
            AccountCode = codeGenerator.GenerateCode()
        };

        await unitOfWork.DetailedAccountRepository.CreateAsync(createDetailedAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}