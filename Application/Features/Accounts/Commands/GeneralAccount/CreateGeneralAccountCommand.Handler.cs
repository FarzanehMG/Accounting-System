using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;
using Domain.Entities;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public class CreateGeneralAccountCommandHandler(IUnitOfWork unitOfWork, IAccountCodeGenerator codeGenerator)
{
    public async Task<OperationResult<bool>> Handle(CreateGeneralAccountCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateGeneralAccountCommandValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var createGeneralAccount = new GeneralLedgerAccountEntity
        {
            AccountTitle = request.Title,
            AccountDescription = request.Description,
            JournalEntryType = request.JournalEntryType,
            IsActive = request.IsActive,
            AccountCode = codeGenerator.GenerateCode()
        };

        await unitOfWork.GeneralLegerAccountRepository.CreateAsync(createGeneralAccount, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<bool>.SuccessResult(true);
    }
}