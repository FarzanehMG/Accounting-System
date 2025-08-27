using Application.Common;
using Application.Extensions;
using Application.Features.Accounts.Queries.GeneralAccount;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public class GetGeneralAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetGeneralAccountByIdQueryResult>> Handel(GetGeneralLedgerAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGeneralLedgerAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetGeneralAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalAccount = await unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (generalAccount is null)
            return OperationResult<GetGeneralAccountByIdQueryResult>.NotFoundResult(nameof(GetGeneralLedgerAccountByIdQuery.Id), "General account not found.");

        return OperationResult<GetGeneralAccountByIdQueryResult>.SuccessResult(new GetGeneralAccountByIdQueryResult
        {
            Id = generalAccount.Id,
            AccountTitle = generalAccount.AccountTitle
        });
    }
}