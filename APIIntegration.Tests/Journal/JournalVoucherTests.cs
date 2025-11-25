using APIIntegration.Tests.Helper;
using Xunit.Abstractions;

namespace APIIntegration.Tests.Journal;

[Collection("Sequential Financial Tests")]
public class JournalVoucherTests(ApiTestSetup setup, ITestOutputHelper output)
{
    [Fact]
    public async Task CreateJournalVoucher_ShouldSucceed_WhenValidDataAndActivePeriod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldStartNumbering_WhenFirstInPeriod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldIncrementNumber_WhenFollowingVoucher()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldBeDraft_ByDefault()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenFinancialPeriodNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenFinancialPeriodNotActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenTransactionTimeIsBeforePeriodStart()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenTransactionTimeIsAfterPeriodEnd()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenMissingRequiredFields()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldPreventDuplicateNumber_WhenParallelExecution()
    {
        throw new NotImplementedException();
    }
}