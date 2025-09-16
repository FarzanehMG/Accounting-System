namespace Application.Tests.Journal;

public class JournalEntryTests
{
    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenCreateJournalEntryCommandLessThanTwoRecords()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAllCommandsNotTargetTheSameJournalVoucherId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_CommandsNotValid()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithSubLedgerAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithDetailedAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAssignIncrementalNumber_PerJournalVoucher()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAllowNullDetailedAccountId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAllowNullDescription()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldNotCommit_WhenOneEntryFailsValidation()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldThrowException_WhenRepositoryFails()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenValidationIsFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenParentJournalVoucherNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenFinancialPeriodInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task WhenAmountIsZeroOrNegative_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchSubLedgerAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task WhenAmountIsZeroOrNegative_ShouldFail_WhenDetailedAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchDetailedAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenAmountNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDescriptionNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenSubLedgerAccountIdNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDetailedAccountIdNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldUpdateModifiedDate_WhenJournalEntryIsUpdated()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldNotCommit_WhenNoFieldsChanged()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenValidationFails()
    {
        throw new NotImplementedException();
    }
}