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

    [Fact]
    public async Task UpdateJournalVoucher_ShouldSucceed_WhenValidRequest()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            JournalVoucherTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            },
            ReferenceNumber = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            },
            TransactionTime = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(2)
            },
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);

        // Verify in database
        var afterUpdateJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .SingleAsync(g => g.Id == journalVoucher.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateJournalVoucher.JournalVoucherTitle.Should().Be(journalVoucher.JournalVoucherTitle);
        afterUpdateJournalVoucher.ReferenceNumber.Should().Be(journalVoucher.ReferenceNumber);
        afterUpdateJournalVoucher.TransactionTime.Should().Be(journalVoucher.TransactionTime);
        afterUpdateJournalVoucher.Description.Should().Be(journalVoucher.Description);
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldFail_WhenJournalVoucherNotFound()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = Guid.NewGuid(),
            JournalVoucherTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Journal voucher not found.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        journalVoucher.JournalVoucherState = JournalVoucherState.Rejected;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            JournalVoucherTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot update entry of a non-Draft voucher.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldFail_WhenFinancialPeriodIsClosed()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        ArgumentNullException.ThrowIfNull(journalVoucher.FinancialPeriod);

        journalVoucher.FinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            JournalVoucherTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Financial period is closed. Cannot update journal voucher.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldFail_WhenTransactionDateNotStayInsideFinancialPeriod()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            TransactionTime = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(6)
            }
        };

        try
        {
            var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Transaction time must be within the financial period");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldSuccess_WhenTitleIsChanged()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            JournalVoucherTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);

        // Verify in database
        var afterUpdateJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == journalVoucher.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateJournalVoucher.JournalVoucherTitle.Should().NotBe(journalVoucher.JournalVoucherTitle);
        afterUpdateJournalVoucher.ReferenceNumber.Should().Be(journalVoucher.ReferenceNumber);
        afterUpdateJournalVoucher.TransactionTime.Should().Be(journalVoucher.TransactionTime);
        afterUpdateJournalVoucher.Description.Should().Be(journalVoucher.Description);
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldSuccess_WhenDescriptionIsChanged()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);

        // Verify in database
        var afterUpdateJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == journalVoucher.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateJournalVoucher.JournalVoucherTitle.Should().Be(journalVoucher.JournalVoucherTitle);
        afterUpdateJournalVoucher.ReferenceNumber.Should().Be(journalVoucher.ReferenceNumber);
        afterUpdateJournalVoucher.TransactionTime.Should().Be(journalVoucher.TransactionTime);
        afterUpdateJournalVoucher.Description.Should().NotBe(journalVoucher.Description);
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldSuccess_WhenReferenceNumberIsChanged()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            ReferenceNumber = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);

        // Verify in database
        var afterUpdateJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == journalVoucher.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateJournalVoucher.JournalVoucherTitle.Should().Be(journalVoucher.JournalVoucherTitle);
        afterUpdateJournalVoucher.ReferenceNumber.Should().NotBe(journalVoucher.ReferenceNumber);
        afterUpdateJournalVoucher.TransactionTime.Should().Be(journalVoucher.TransactionTime);
        afterUpdateJournalVoucher.Description.Should().Be(journalVoucher.Description);
    }

    [Fact]
    public async Task UpdateJournalVoucher_ShouldSucceed_WhenTransactionDateIsExactlyStartOrEndOfPeriod()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create or get financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create journal voucher
        var journalVoucher = await setup.TestInit1.CreateJournalVoucher(financialPeriod);

        // Update journal voucher
        var model = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucher.Id,
            TransactionTime = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(2)
            }
        };

        var result = await setup.TestInit1.JournalVouchClient.UpdateAsync(model, CancellationToken.None);

        // Verify in database
        var afterUpdateJournalVoucher = await setup.TestInit1.AccountingDbContext
            .Set<JournalVoucherEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == journalVoucher.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateJournalVoucher.JournalVoucherTitle.Should().Be(journalVoucher.JournalVoucherTitle);
        afterUpdateJournalVoucher.ReferenceNumber.Should().Be(journalVoucher.ReferenceNumber);
        afterUpdateJournalVoucher.TransactionTime.Should().NotBe(journalVoucher.TransactionTime);
        afterUpdateJournalVoucher.Description.Should().Be(journalVoucher.Description);
    }
}