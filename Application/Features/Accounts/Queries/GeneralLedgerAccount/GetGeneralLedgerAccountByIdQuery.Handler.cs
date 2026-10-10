using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public class GetGeneralLedgerAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetGeneralLedgerAccountByIdQueryResult>> Handle(GetGeneralLedgerAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGeneralLedgerAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetGeneralLedgerAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalAccount = await unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (generalAccount is null)
            return OperationResult<GetGeneralLedgerAccountByIdQueryResult>.NotFoundResult(nameof(GetGeneralLedgerAccountByIdQuery.Id), "General account not found.");

        return OperationResult<GetGeneralLedgerAccountByIdQueryResult>.SuccessResult(generalAccount.ToDto());
    }
}