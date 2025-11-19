using APIIntegration.Tests.Helper;
using Domain.Entities.FinancialPeriod;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using UpdateFinancialPeriodCommand = GenerateApi.UpdateFinancialPeriodCommand;

namespace APIIntegration.Tests.FinancialPeriod;

public class FinancialPeriodTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateFinancialPeriod__ShouldSuccess_WhenRequestIsValid()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(100),
            EndDate = DateTime.UtcNow.AddDays(100)
        };
        await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);

        // Assert API response
        output.WriteLine($"Created financialPeriod via API: {model.FinancialPeriodTitle}");

        // Verify in database
        var createdFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleOrDefaultAsync(g => g.FinancialPeriodTitle == model.FinancialPeriodTitle && g.StartDate == model.StartDate && g.EndDate == model.EndDate);

        // Assert
        Assert.NotNull(createdFinancialPeriod);
        Assert.Equal(model.FinancialPeriodTitle, createdFinancialPeriod.FinancialPeriodTitle);
        Assert.Equal(model.StartDate, createdFinancialPeriod.StartDate);
        Assert.Equal(model.EndDate, createdFinancialPeriod.EndDate);
        output.WriteLine($"Verified FinancialPeriod exists in DB: {createdFinancialPeriod.Id}");
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenEndDateLessThanStartDate()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(100),
            EndDate = DateTime.UtcNow
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The end date must be later than the start date of the financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenPeriodOverlapWithExistingFinancialPeriod()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The selected period overlaps with an existing financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenHaveActiveExistingFinancialPeriod()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(100),
            EndDate = DateTime.UtcNow.AddDays(101)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("You must close the current active financial period before creating a new one.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenEndDateEqualsStartDate()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var exactDate = DateTime.UtcNow.AddDays(100).Date;

        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = exactDate,
            EndDate = exactDate
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The end date must be later than the start date of the financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenFinancialPeriodTitleIsNull()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = "",
            StartDate = DateTime.UtcNow.AddDays(100),
            EndDate = DateTime.UtcNow.AddDays(101)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("'Financial Period Title' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenStartDateIsNull()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.MinValue,
            EndDate = DateTime.UtcNow.AddDays(101)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("'Start Date' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenEndDateIsNull()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get active financial period
        var activeFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.IsActive);

        // Close active financial period
        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(101),
            EndDate = DateTime.MinValue
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("'End Date' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenPeriodOverlapsOnlyAtStartDate()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(101)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The selected period overlaps with an existing financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateFinancialPeriod_ShouldFail_WhenPeriodOverlapsOnlyAtEndDate()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(80),
            EndDate = DateTime.UtcNow.AddDays(90)
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.CreateAsync(model);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The selected period overlaps with an existing financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenOnlyTitleIsProvided()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            FinancialPeriodTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().NotBe(financialPeriod.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().Be(financialPeriod.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().Be(financialPeriod.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenOnlyStartDateIsProvided()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            StartDate = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(2)
            }
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().Be(financialPeriod.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().NotBe(financialPeriod.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().Be(financialPeriod.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenOnlyEndDateIsProvided()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod.EndDate.AddDays(2)
            }
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().Be(financialPeriod.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().Be(financialPeriod.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().NotBe(financialPeriod.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenAllFieldsProvidedAndNoJournalEntries()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update isActive
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create active financial period
        var financialPeriod2 = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod2.Id,
            FinancialPeriodTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            },
            StartDate = new PatchOfDateTime
            {
                Value = financialPeriod2.StartDate.AddDays(2)
            },
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod2.EndDate.AddDays(2)
            }
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod2.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().NotBe(financialPeriod2.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().NotBe(financialPeriod2.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().NotBe(financialPeriod2.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod2.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldNotChange_WhenNoFieldsProvided()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().Be(financialPeriod.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().Be(financialPeriod.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().Be(financialPeriod.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenFinancialPeriodDoesNotExist()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = Guid.NewGuid()
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Financial Period Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenPeriodIsClosed()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update isActive
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            FinancialPeriodTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Cannot update a closed financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenOverlapsWithAnotherPeriod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenJournalEntriesExistAndStartDateIsChanged()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal voucher
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod);

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            StartDate = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(4)
            }
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("annot change StartDate because journal entries already exist in the period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenJournalVouchersExistAndEndDateIsChanged()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal voucher
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod);

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod.EndDate.AddDays(4)
            }
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Cannot change EndDate because journal entries already exist in the period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenJournalVouchersExistAndOnlyTitleIsChanged()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update isActive
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create active financial period
        var financialPeriod2 = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal voucher
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod2);

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod2.Id,
            FinancialPeriodTitle = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);

        // Get financial period
        var afterUpdateFinancialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == financialPeriod2.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateFinancialPeriod.FinancialPeriodTitle.Should().NotBe(financialPeriod2.FinancialPeriodTitle);
        afterUpdateFinancialPeriod.StartDate.Should().Be(financialPeriod2.StartDate);
        afterUpdateFinancialPeriod.EndDate.Should().Be(financialPeriod2.EndDate);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod2.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenEndDateEqualsStartDate()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update isActive
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create active financial period
        var financialPeriod2 = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod2.Id,
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod2.StartDate
            }
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("End date must be later than start date.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenEndDateLessThanStartDate()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate.AddDays(-5)
            }
        };

        try
        {
            var result = await setup.TestInit1.FinancialPeriodClient.UpdateAsync(updateFinancialPeriodCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("End date must be later than start date.");
            ex.StatusCode.Should().Be(400);
        }
    }
}