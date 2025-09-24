using Bogus;
using Domain.Entities.Accounts;
using Domain.Enums;

namespace Application.Tests.Journal;

public static class Helper
{
    public static SubLedgerAccountEntity CreateSubLedgerAccount(Guid id, JournalEntryType type, Faker faker, bool? isActive = true)
    {
        return new SubLedgerAccountEntity
        {
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = type,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = isActive != null && isActive.Value
        };
    }

    public static DetailedAccountEntity CreateDetailedAccount(Guid id, Guid subLedgerId, JournalEntryType type, Faker faker, bool? isActive = true)
    {
        return new DetailedAccountEntity
        {
            SubLedgerAccountId = subLedgerId,
            JournalEntryType = type,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = isActive != null && isActive.Value
        };
    }
}