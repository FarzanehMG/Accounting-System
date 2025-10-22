using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using CreateGeneralLedgerAccountApiModel = GenerateApi.CreateGeneralLedgerAccountApiModel;

namespace APIIntegration.Tests.Accounts;

public class GeneralLedgerAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act
        await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created generalLedgerAccount via API: {model.Title}");

        // Verify in database
        var createdGeneralAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        Assert.NotNull(createdGeneralAccount);
        Assert.Equal(model.Code, createdGeneralAccount.AccountCode);
        Assert.Equal(model.Title, createdGeneralAccount.AccountTitle);
        output.WriteLine($"Verified GeneralLedgerAccount exists in DB: {createdGeneralAccount.Id}");
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeAndTitleExists()
    {
        throw new NotImplementedException();
        // ErrorMessage: An active account with this title already exists in the selected group.
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldNotFail_WhenInActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldReturnInactiveGroupAccountId_WhenInactiveGroupWithSameTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldSuccess_WhenFinancialPeriodIsActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenTitleIsNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenJournalEntryTypeIsNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenCodeIsNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenGeneralLedgerAccountDoesNotExist()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSucceed_WhenTitleIsProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeAndTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldSucceed_WhenCodeIsProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSucceed_WhenJournalEntryTypeIsProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenJournalEntriesExistAndJournalEntryTypeIsChanged()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenActiveAccountWithSameJournalEntryTypeExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenValidatorFails()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenFinancialPeriodIsClosed()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenJournalTypeChangedAndHasJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeJournalType_WhenNoJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenCodeIsDuplicate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeCode_WhenValidAndNoJournals()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenCodeTooShort()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeTitle_WhenUnique()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeIsActive_WhenActivatingAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeDescription_WhenProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldNotChangeDescription_WhenNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenGroupAccountInactive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeGroupAccount_WhenActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFailOrChangeFields_WhenMultipleUpdatesProvided_WithJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByTitle()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByJournalEntryType()
    {
        throw new NotImplementedException();
    }
}