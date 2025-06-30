using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GeneralAccount;

public class GetGeneralAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetGeneralAccountByIdQueryResult>> Handel(GetGeneralAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGeneralAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetGeneralAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalAccount = await unitOfWork.GeneralLegerAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (generalAccount is null)
            return OperationResult<GetGeneralAccountByIdQueryResult>.NotFoundResult(nameof(GetGeneralAccountByIdQuery.Id), "General account not found.");

        return OperationResult<GetGeneralAccountByIdQueryResult>.SuccessResult(new GetGeneralAccountByIdQueryResult(generalAccount.Id, generalAccount.AccountTitle));
    }
}