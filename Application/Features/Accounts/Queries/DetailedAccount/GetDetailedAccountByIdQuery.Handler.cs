using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetDetailedAccountByIdQueryResult>> Handle(GetDetailedAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetDetailedAccountByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetDetailedAccountByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (detailedAccount is null)
            return OperationResult<GetDetailedAccountByIdQueryResult>.NotFoundResult(nameof(GetDetailedAccountByIdQuery.Id), "Detailed account not found.");

        return OperationResult<GetDetailedAccountByIdQueryResult>.SuccessResult(detailedAccount.ToDto());
    }
}