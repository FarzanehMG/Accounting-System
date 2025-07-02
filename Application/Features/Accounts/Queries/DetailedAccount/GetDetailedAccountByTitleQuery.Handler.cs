using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountByTitleQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<List<GetDetailedAccountByTitleQueryResult>>> Handle(GetDetailedAccountByTitleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetDetailedAccountByTitleQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<List<GetDetailedAccountByTitleQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetByTitleAsync(request.AccountTitle, cancellationToken);

        if (!detailedAccount.Any())
            return OperationResult<List<GetDetailedAccountByTitleQueryResult>>.NotFoundResult(nameof(GetDetailedAccountByTitleQuery), "Detailed account not found.");

        return OperationResult<List<GetDetailedAccountByTitleQueryResult>>
            .SuccessResult(detailedAccount.Select(x => new GetDetailedAccountByTitleQueryResult(x.Id, x.AccountTitle))
                .ToList());
    }
}