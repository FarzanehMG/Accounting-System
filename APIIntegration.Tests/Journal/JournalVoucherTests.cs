using APIIntegration.Tests.Helper;
using Domain.Entities.Journal;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace APIIntegration.Tests.Journal;

[Collection("Sequential Financial Tests")]
public class JournalVoucherTests(ApiTestSetup setup, ITestOutputHelper output)
{
    [Fact]
    public async Task CreateJournalVoucher_ShouldSucceed_WhenValidDataAndActivePeriod()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var model = new CreateJournalVoucherApiModel
        {
            FinancialPeriodId = financialPeriod.Id,
            JournalVoucherTitle = Guid.NewGuid().ToString(),
            ReferenceNumber = Guid.NewGuid().ToString(),
            TransactionTime = financialPeriod.StartDate.AddDays(1),
            CreatedByUserId = Guid.NewGuid()
        };
        await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created journal voucher via API: {model.JournalVoucherTitle}");

        // Verify in database
        var createdJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .SingleOrDefaultAsync(g => g.JournalVoucherTitle == model.JournalVoucherTitle && g.ReferenceNumber == model.ReferenceNumber);

        // Assert
        Assert.NotNull(createdJournalVoucher);
        Assert.Equal(model.JournalVoucherTitle, createdJournalVoucher.JournalVoucherTitle);
        Assert.Equal(model.ReferenceNumber, createdJournalVoucher.ReferenceNumber);
        Assert.Equal(model.TransactionTime, createdJournalVoucher.TransactionTime);
        Assert.Equal(model.FinancialPeriodId, createdJournalVoucher.FinancialPeriodId);
        output.WriteLine($"Verified journal voucher exists in DB: {createdJournalVoucher.Id}");

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