using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLedgerAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetSubLedgerAccountByIdQueryResult>> Handle(GetSubLedgerAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSubLedgerAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetSubLedgerAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var subLedgerAccount = await unitOfWork.SubLedgerAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (subLedgerAccount is null)
            return OperationResult<GetSubLedgerAccountByIdQueryResult>.NotFoundResult(nameof(GetSubLedgerAccountByIdQuery.Id), "SubLedger account not found");

        return OperationResult<GetSubLedgerAccountByIdQueryResult>.SuccessResult(subLedgerAccount.ToDto());
    }
}