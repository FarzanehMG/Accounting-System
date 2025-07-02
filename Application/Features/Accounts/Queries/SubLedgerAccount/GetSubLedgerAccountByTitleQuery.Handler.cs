using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLedgerAccountByTitleQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<List<GetSubLedgerAccountByTitleQueryResult>>> Handle(GetSubLedgerAccountByTitleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSubLedgerAccountByTitleQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<List<GetSubLedgerAccountByTitleQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var subLedgerAccount = await unitOfWork.SubLegerAccountRepository.GetByTitleAsync(request.AccountTitle, cancellationToken);

        if (!subLedgerAccount.Any())
            return OperationResult<List<GetSubLedgerAccountByTitleQueryResult>>.NotFoundResult(nameof(GetSubLedgerAccountByTitleQuery), "SubLedger account not found");

        return OperationResult<List<GetSubLedgerAccountByTitleQueryResult>>.SuccessResult(subLedgerAccount.Select(x => new GetSubLedgerAccountByTitleQueryResult(x.Id, x.AccountTitle)).ToList());
    }
}