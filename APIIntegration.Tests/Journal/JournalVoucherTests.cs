using APIIntegration.Tests.Helper;
using Domain.Entities.Journal;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using JournalVoucherState = Domain.Enums.JournalVoucherState;

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
        Assert.Equal(1, createdJournalVoucher.JournalVoucherNumber);
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldIncrementNumber_WhenFollowingVoucher()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        await setup.TestInit1.CreateJournalVoucher(financialPeriod);

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
        Assert.Equal(2, createdJournalVoucher.JournalVoucherNumber);
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldBeDraft_ByDefault()
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
        Assert.Equal(1, createdJournalVoucher.JournalVoucherNumber);
        Assert.Equal(JournalVoucherState.Draft, createdJournalVoucher.JournalVoucherState);
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenFinancialPeriodNotFound()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create journal voucher
        var model = new CreateJournalVoucherApiModel
        {
            FinancialPeriodId = Guid.NewGuid(),
            JournalVoucherTitle = Guid.NewGuid().ToString(),
            ReferenceNumber = Guid.NewGuid().ToString(),
            TransactionTime = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Financial period not found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenFinancialPeriodNotActive()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create journal voucher
        var model = new CreateJournalVoucherApiModel
        {
            FinancialPeriodId = financialPeriod.Id,
            JournalVoucherTitle = Guid.NewGuid().ToString(),
            ReferenceNumber = Guid.NewGuid().ToString(),
            TransactionTime = financialPeriod.StartDate.AddDays(1),
            CreatedByUserId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("This financial period is not active.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenTransactionTimeIsBeforePeriodStart()
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
            TransactionTime = financialPeriod.StartDate.AddDays(-1),
            CreatedByUserId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Transaction time must be within the selected financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenTransactionTimeIsAfterPeriodEnd()
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
            TransactionTime = financialPeriod.EndDate.AddDays(1),
            CreatedByUserId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Transaction time must be within the selected financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldFail_WhenMissingRequiredFields()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var model = new CreateJournalVoucherApiModel
        {
            FinancialPeriodId = financialPeriod.Id,
            JournalVoucherTitle = "",
            ReferenceNumber = Guid.NewGuid().ToString(),
            TransactionTime = financialPeriod.StartDate.AddDays(1),
            CreatedByUserId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("'Journal Voucher Title' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateJournalVoucher_ShouldPreventDuplicateNumber_WhenParallelExecution()
    {
        throw new NotImplementedException();
    }
}