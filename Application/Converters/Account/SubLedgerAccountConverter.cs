using Application.Features.Accounts.Queries.SubLedgerAccount;
using Domain.Entities;

namespace Application.Converters.Account;

public static class SubLedgerAccountConverter
{
    public static GetSubLedgerAccountByIdQueryResult ToDto(this SubLedgerAccountEntity entity)
    {
        return new GetSubLedgerAccountByIdQueryResult
        {
            Id = entity.Id,
            AccountTitle = entity.AccountTitle
        };
    }

    public static IReadOnlyList<GetSubLedgerAccountsQueryResult> ToDto(this IReadOnlyList<SubLedgerAccountEntity> entity)
    {
        return entity
            .Select(x => new GetSubLedgerAccountsQueryResult
            {
                AccountTitle = x.AccountTitle,
                AccountCode = x.AccountCode,
                IsActive = x.IsActive,
                JournalEntryType = x.JournalEntryType,
                GeneralLedgerAccountId = x.GeneralLedgerAccountId
            }).ToList()
            .AsReadOnly();
    }
}