using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Features.Accounts.Queries.DetailedAccount.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountsQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetDetailedAccountsQueryResult>>> Handle(GetDetailedAccountsQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetDetailedAccountsQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetDetailedAccountsQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var detailedAccountFilter = new DetailedAccountFilter
        {
            Title = request.Title,
            SubLedgerAccountId = request.SubLedgerAccountId,
            Code = request.Code,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType
        };
        var detailedAccount = await unitOfWork.DetailedAccountRepository.GetAllAsync(detailedAccountFilter, cancellationToken);

        if (!detailedAccount.Any())
            return OperationResult<IReadOnlyList<GetDetailedAccountsQueryResult>>.NotFoundResult(nameof(GetDetailedAccountsQueryResult), "Detailed account not found.");

        return OperationResult<IReadOnlyList<GetDetailedAccountsQueryResult>>
            .SuccessResult(detailedAccount.ToDto());
    }
}