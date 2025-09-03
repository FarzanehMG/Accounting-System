using Application.Common;
using Application.Extensions;
using Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLedgerAccountsQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetSubLedgerAccountsQueryResult>>> Handle(GetSubLedgerAccountsQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSubLedgerAccountsQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetSubLedgerAccountsQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var subLedgerAccountFilter = new SubLedgerAccountFilter
        {
            Title = request.Title,
            GeneralLedgerAccountId = request.GeneralLedgerAccountId,
            Code = request.Code,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType
        };
        var subLedgerAccount = await unitOfWork.SubLedgerAccountRepository.GetAllAsync(subLedgerAccountFilter, cancellationToken);

        if (!subLedgerAccount.Any())
            return OperationResult<IReadOnlyList<GetSubLedgerAccountsQueryResult>>.NotFoundResult(nameof(GetSubLedgerAccountsQueryResult), "SubLedger account not found.");

        return OperationResult<IReadOnlyList<GetSubLedgerAccountsQueryResult>>
            .SuccessResult(subLedgerAccount.Select(x => new GetSubLedgerAccountsQueryResult
            {
                    AccountTitle = x.AccountTitle,
                    AccountCode = x.AccountCode,
                    IsActive = x.IsActive,
                    JournalEntryType = x.JournalEntryType,
                    GeneralLedgerAccountId = x.GeneralLedgerAccountId
                }).ToList()
                .AsReadOnly());
    }
}