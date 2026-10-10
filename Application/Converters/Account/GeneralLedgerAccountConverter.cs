using Application.Features.Accounts.Queries.GeneralLedgerAccount;
using Domain.Entities.Accounts;

namespace Application.Converters.Account;

public static class GeneralLedgerAccountConverter
{
    public static GetGeneralLedgerAccountByIdQueryResult ToDto(this GeneralLedgerAccountEntity entity)
    {
        return new GetGeneralLedgerAccountByIdQueryResult
        {
            Id = entity.Id,
            AccountTitle = entity.AccountTitle
        };
    }

    public static IReadOnlyList<GetGeneralLedgerAccountsQueryResult> ToDto(this IReadOnlyList<GeneralLedgerAccountEntity> entity)
    {
        return entity
            .Select(x => new GetGeneralLedgerAccountsQueryResult
            {
                AccountTitle = x.AccountTitle,
                AccountCode = x.AccountCode,
                IsActive = x.IsActive,
                JournalEntryType = x.JournalEntryType,
                GroupAccountId = x.GroupAccountId
            }).ToList()
            .AsReadOnly();
    }
}