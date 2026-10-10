using APIIntegration.Tests.Helper;
using Application.Features.FinancialPeriod.Queries.Dtos;
using Domain.Entities.FinancialPeriod;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using UpdateFinancialPeriodCommand = GenerateApi.UpdateFinancialPeriodCommand;

namespace APIIntegration.Tests.FinancialPeriod;

[Collection("Sequential Financial Tests")]
public class FinancialPeriodTests(ApiTestSetup setup, ITestOutputHelper output)
{
    [Fact]
    public async Task CreateFinancialPeriod_ShouldSuccess_WhenRequestIsValid()
    {
        // Get or create financial period
        var activeFinancialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = activeFinancialPeriod.EndDate.AddDays(1),
            EndDate = activeFinancialPeriod.EndDate.AddDays(3)
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
        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate =DateTime.UtcNow.Date
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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Get or create financial period
        var activeFinancialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        activeFinancialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = activeFinancialPeriod.StartDate,
            EndDate = activeFinancialPeriod.EndDate
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
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = financialPeriod.EndDate.AddDays(1),
            EndDate = financialPeriod.EndDate.AddDays(2)
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
        // Create financial period
        var exactDate = DateTime.UtcNow.AddDays(1);

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
        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2)
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
        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.MinValue,
            EndDate = DateTime.UtcNow.AddDays(2)
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
        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(1),
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
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = financialPeriod.EndDate.AddDays(-2),
            EndDate = financialPeriod.EndDate.AddDays(2)
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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create financial period
        var model = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = financialPeriod.EndDate.AddDays(-2),
            EndDate = financialPeriod.EndDate.AddDays(-1)
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
        afterUpdateFinancialPeriod.StartDate.Date.Should().Be(financialPeriod.StartDate.Date);
        afterUpdateFinancialPeriod.EndDate.Date.Should().Be(financialPeriod.EndDate.Date);
        afterUpdateFinancialPeriod.IsActive.Should().Be(financialPeriod.IsActive);
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldSucceed_WhenOnlyStartDateIsProvided()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
            responseContent.Should().Contain("Financial period not found.");
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
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create active financial period
        var createFinancialPeriod = new CreateFinancialPeriodApiModel
        {
            FinancialPeriodTitle = Guid.NewGuid().ToString(),
            StartDate = financialPeriod.EndDate.AddDays(1),
            EndDate = financialPeriod.EndDate.AddDays(2)
        };
        await setup.TestInit1.FinancialPeriodClient.CreateAsync(createFinancialPeriod);

        var financialPeriod2 = await setup.TestInit1.AccountingDbContext.Set<FinancialPeriodEntity>()
            .SingleAsync(x => x.FinancialPeriodTitle == createFinancialPeriod.FinancialPeriodTitle && x.StartDate == createFinancialPeriod.StartDate);


        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod2.Id,
            StartDate = new PatchOfDateTime
            {
                Value = financialPeriod.EndDate.AddDays(-2)
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
            responseContent.Should().Contain("The selected period overlaps with an existing financial period.");
            ex.StatusCode.Should().Be(400);
        }
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
            responseContent.Should().Contain("Cannot change StartDate because journal entries already exist in the period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateFinancialPeriod_ShouldFail_WhenJournalVouchersExistAndEndDateIsChanged()
    {
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();

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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriod.Id,
            EndDate = new PatchOfDateTime
            {
                Value = financialPeriod.StartDate
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
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();
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

    [Fact]
    public async Task GetAllFinancialPeriods_ShouldSucceed_WhenNoFiltersApplied()
    {
        // Create active financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get financial period
        var original = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .ToListAsync();

        // Get all financial period
        var result = await setup.TestInit1.FinancialPeriodClient.GetAllAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllFinancialPeriods_ShouldSucceed_WhenFilteredByTitle()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get financial period
        var original = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .Where(x => x.FinancialPeriodTitle == financialPeriod.FinancialPeriodTitle)
            .ToListAsync();

        // Get all financial period
        var filterFinancialPeriod = new FinancialPeriodFilter
        {
            Title = financialPeriod.FinancialPeriodTitle
        };
        var result = await setup.TestInit1.FinancialPeriodClient.GetAllAsync(filterFinancialPeriod.Title);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllFinancialPeriods_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get all financial period
        var filterFinancialPeriod = new FinancialPeriodFilter
        {
            IsActive = financialPeriod.IsActive
        };
        var result = await setup.TestInit1.FinancialPeriodClient.GetAllAsync(isActive: filterFinancialPeriod.IsActive);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllFinancialPeriods_ShouldSucceed_WhenFilteredByStartDate()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get financial period
        var original = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .Where(x=>x.StartDate.Date == financialPeriod.StartDate.Date)
            .ToListAsync();

        // Get all financial period
        var filterFinancialPeriod = new FinancialPeriodFilter
        {
            StartDate = financialPeriod.StartDate
        };
        var result = await setup.TestInit1.FinancialPeriodClient.GetAllAsync(startDate: filterFinancialPeriod.StartDate);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllFinancialPeriods_ShouldSucceed_WhenFilteredByEndDate()
    {
        // Create active financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Get financial period
        var original = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .AsNoTracking()
            .Where(x => x.EndDate.Date == financialPeriod.EndDate.Date)
            .ToListAsync();

        // Get all financial period
        var filterFinancialPeriod = new FinancialPeriodFilter
        {
            EndDate = financialPeriod.EndDate
        };
        var result = await setup.TestInit1.FinancialPeriodClient.GetAllAsync(endDate: filterFinancialPeriod.EndDate);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }
}