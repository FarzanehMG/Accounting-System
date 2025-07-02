using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLegerAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetSubLegerAccountByIdQueryResult>> Handle(GetSubLedgerAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSubLedgerAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetSubLegerAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var subLedgerAccount = await unitOfWork.SubLegerAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (subLedgerAccount is null)
            return OperationResult<GetSubLegerAccountByIdQueryResult>.NotFoundResult(nameof(GetSubLedgerAccountByIdQuery), "SubLedger account not found");

        return OperationResult<GetSubLegerAccountByIdQueryResult>.SuccessResult(new GetSubLegerAccountByIdQueryResult(subLedgerAccount.Id, subLedgerAccount.AccountTitle));
    }
}