using Application.Common;
using Application.Converters.Account;
using Application.Extensions;
using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountsQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetGroupAccountsQueryResult>>> Handle(GetGroupAccountsQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetGroupAccountsQueryValidator();
        var validateResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetGroupAccountsQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var groupAccounts = await unitOfWork.GroupAccountRepository.GetAllAsync(new GroupAccountFilter
        {
            Title = request.Title,
            Code = request.Code,
            IsActive = request.IsActive,
            JournalEntryType = request.JournalEntryType
        }, cancellationToken);

        if (!groupAccounts.Any())
            return OperationResult<IReadOnlyList<GetGroupAccountsQueryResult>>.NotFoundResult(nameof(GetGroupAccountsQueryResult), "Group account not found.");

        return OperationResult<IReadOnlyList<GetGroupAccountsQueryResult>>.SuccessResult(groupAccounts.ToDto());
    }
}