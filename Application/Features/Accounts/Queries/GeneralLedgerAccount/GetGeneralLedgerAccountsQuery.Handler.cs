using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public class GetGeneralLedgerAccountsQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetGeneralLedgerAccountsQueryResult>>> Handle(GetGeneralLedgerAccountsQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGeneralLedgerAccountsQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetGeneralLedgerAccountsQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var generalLedgerAccountFilter = new GeneralLedgerAccountFilter
        {
            Title = request.Title,
            GroupAccountId = request.GroupAccountId,
            Code = request.Code,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType
        };
        var generalAccount = await unitOfWork.GeneralLedgerAccountRepository.GetAllAsync(generalLedgerAccountFilter, cancellationToken);

        if (!generalAccount.Any())
            return OperationResult<IReadOnlyList<GetGeneralLedgerAccountsQueryResult>>.NotFoundResult(nameof(GetGeneralLedgerAccountsQueryResult), "General account not found.");

        return OperationResult<IReadOnlyList<GetGeneralLedgerAccountsQueryResult>>
            .SuccessResult(generalAccount.ToDto());
    }
}