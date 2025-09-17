using Application.Features.Accounts.Queries.DetailedAccount;
using Domain.Entities;

namespace Application.Converters.Account;

public static class DetailedAccountConverter
{
    public static GetDetailedAccountByIdQueryResult ToDto(this DetailedAccountEntity entity)
    {
        return new GetDetailedAccountByIdQueryResult
        {
            Id = entity.Id,
            AccountTitle = entity.AccountTitle
        };
    }

    public static IReadOnlyList<GetDetailedAccountsQueryResult> ToDto(this IReadOnlyList<DetailedAccountEntity> entity)
    {
        return entity
            .Select(x => new GetDetailedAccountsQueryResult
            {
                AccountTitle = x.AccountTitle,
                AccountCode = x.AccountCode,
                IsActive = x.IsActive,
                JournalEntryType = x.JournalEntryType,
                SubLedgerAccountId = x.SubLedgerAccountId
            }).ToList()
            .AsReadOnly();
    }
}