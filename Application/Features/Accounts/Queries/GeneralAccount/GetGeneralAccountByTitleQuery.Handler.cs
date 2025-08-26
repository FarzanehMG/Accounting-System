using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GeneralAccount;

public class GetGeneralAccountByTitleQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<List<GetGeneralAccountByTitleQueryResult>>> Handle(GetGeneralAccountByTitleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGeneralAccountByTitleQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<List<GetGeneralAccountByTitleQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalAccount = await unitOfWork.GeneralLedgerAccountRepository.GetByTitleAsync(request.AccountTitle, cancellationToken);

        if (!generalAccount.Any())
            return OperationResult<List<GetGeneralAccountByTitleQueryResult>>.NotFoundResult(nameof(GetGeneralAccountByTitleQuery), "General account not found.");

        return OperationResult<List<GetGeneralAccountByTitleQueryResult>>
            .SuccessResult(generalAccount.Select(x=>new GetGeneralAccountByTitleQueryResult(x.Id, x.AccountTitle))
                .ToList());
    }
}