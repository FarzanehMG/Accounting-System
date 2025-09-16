using Bogus;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Journal;

public static class Helper
{
    public static SubLedgerAccountEntity CreateSubLedgerAccount(Guid id, JournalEntryType type, Faker faker) => new()
    {
        Id = id,
        AccountTitle = faker.Finance.AccountName(),
        AccountCode = faker.Finance.Account(),
        JournalEntryType = type,
        GeneralLedgerAccountId = Guid.NewGuid(),
        IsActive = true
    };

    public static DetailedAccountEntity CreateDetailedAccount(Guid id, Guid subLedgerId, JournalEntryType type, Faker faker) => new()
    {
        Id = id,
        SubLedgerAccountId = subLedgerId,
        JournalEntryType = type,
        AccountTitle = faker.Finance.AccountName(),
        AccountCode = faker.Finance.Account(),
        IsActive = true
    };
}