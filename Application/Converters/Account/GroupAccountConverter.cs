using Application.Features.Accounts.Queries.GroupAccount;
using Domain.Entities.Accounts;

namespace Application.Converters.Account;

public static class GroupAccountConverter
{
    public static GetGroupAccountByIdQueryResult ToDto(this GroupAccountEntity entity)
    {
        return new GetGroupAccountByIdQueryResult
        {
            Id = entity.Id,
            AccountTitle = entity.AccountTitle
        };
    }

    public static IReadOnlyList<GetGroupAccountsQueryResult> ToDto(this IReadOnlyList<GroupAccountEntity> entity)
    {
        return entity
            .Select(x => new GetGroupAccountsQueryResult
            {
                AccountTitle = x.AccountTitle,
                AccountCode = x.AccountCode,
                IsActive = x.IsActive,
                AccountDescription = x.AccountDescription,
                JournalEntryType = x.JournalEntryType
            }).ToList()
            .AsReadOnly();
    }
}