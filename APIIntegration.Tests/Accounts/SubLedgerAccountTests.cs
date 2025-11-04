using API.Controllers.Models.Accounts;
using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using CreateSubLedgerAccountApiModel = GenerateApi.CreateSubLedgerAccountApiModel;

namespace APIIntegration.Tests.Accounts;

public class SubLedgerAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateSubLedgerAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created subLedgerAccount via API: {model.Title}");

        // Verify in database
        var createdSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Assert
        Assert.NotNull(createdSubLedgerAccount);
        Assert.Equal(model.Code, createdSubLedgerAccount.AccountCode);
        Assert.Equal(model.Title, createdSubLedgerAccount.AccountTitle);
        output.WriteLine($"Verified SubLedgerAccount exists in DB: {createdSubLedgerAccount.Id}");
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenGeneralLedgerNotFound()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = Guid.NewGuid()
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("GeneralLedger not found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenJournalEntryTypeNotSameWithGeneralLedgerAccount()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Debit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("SubLedger account must have the same nature (JournalEntryType) as its GeneralLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenGeneralLedgerAccountInActive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The GeneralLedger account must be active.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenActiveSubLedgerAccountWithSameGeneralLedgerAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = subLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("An active account with this title already exists in the selected generalLedger.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldSuccess_WhenInActiveSubLedgerAccountWithSameGeneralLedgerAccountAndJournalEntryTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        // Create subLedger account
        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = subLedgerAccount.AccountCode,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Account code already exists.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldReturnInactiveSubLedgerAccountId_WhenInactiveSubLedgerWithSameTitleAndGeneralLedgerAccountAndTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = subLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {subLedgerAccount.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{subLedgerAccount.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldSuccess_WhenInactiveSubLedgerWithSameTitleAndTypeExistsInDifferentGeneralLedgerAccount()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = subLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount2.Id
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().NotBeNull(result.Message);
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenTitleIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = "",
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'Title' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenJournalEntryTypeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = (JournalEntryType)3,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("JournalEntryType must be one of the valid enum values: Debit or Credit.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenCodeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'Code' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create SubLedger account
        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("No active financial period found.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenSubLedgerAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = Guid.NewGuid()
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("SubLedger Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
        afterUpdateSubLedgerAccount.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldSucceed_WhenTitleIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountTitle.Should().NotBe(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenActiveSubLedgerAccountWithSameGeneralLedgerAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);
        var subLedgerAccount2 = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount2.Id,
            Title = new PatchOfString
            {
                Value = subLedgerAccount.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected generalLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldSucceed_WhenCodeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().NotBe(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldSucceed_WhenJournalEntryTypeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().NotBe(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenJournalEntriesExistAndJournalEntryTypeIsChanged()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create subLedger account
        var subLedgerAccountCredit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccountDebit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id, subLedgerAccountCredit, subLedgerAccountDebit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccountCredit.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot change Journal Entry Type for an account with existing journal entries.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenValidatorFails()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = Guid.Empty,
            Title = new PatchOfString
            {
                Value = ""
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'Sub Ledger Account Id' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = Guid.NewGuid(),
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("SubLedger Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create subLedger account
        var subLedgerAccountCredit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccountDebit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id, subLedgerAccountCredit, subLedgerAccountDebit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot change account code for an account with journal entries.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenCodeIsDuplicate()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create subLedger account
        var subLedgerAccountCredit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccountDebit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = subLedgerAccountDebit.AccountCode
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Account code already exists. Please provide a unique code.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccount2 = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = subLedgerAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected generalLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccount2 = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit, isActive: false);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = subLedgerAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {subLedgerAccount2.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{subLedgerAccount2.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create subLedger account
        var subLedgerAccountCredit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);
        var subLedgerAccountDebit = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id, subLedgerAccountCredit, subLedgerAccountDebit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccountCredit.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot deactivate an account with existing journal entries.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().NotBe(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldChangeDescription_WhenProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountDescription.Should().NotBeNull();
        afterUpdateSubLedgerAccount.AccountDescription.Should().NotBe(subLedgerAccount.AccountDescription);
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().Be(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldNotChangeDescription_WhenNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Description = new PatchOfString
            {
                Value = null
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountDescription.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenGeneralLedgerAccountInactive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLEdger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            GeneralLedgerAccountId = new PatchOfGuid
            {
                Value = Guid.NewGuid()
            }
        };

        // InActive generalLedger account
        generalLedgerAccount.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot assign account to an inactive or non-existent generalLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldChangeGroupAccount_WhenActive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            GeneralLedgerAccountId = new PatchOfGuid
            {
                Value = generalLedgerAccount2.Id
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountDescription.Should().Be(subLedgerAccount.AccountDescription);
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().Be(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().NotBe(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            GeneralLedgerAccountId = new PatchOfGuid
            {
                Value = generalLedgerAccount2.Id
            },
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);

        // Get subLedger account
        var afterUpdateSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == subLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateSubLedgerAccount.AccountDescription.Should().Be(subLedgerAccount.AccountDescription);
        afterUpdateSubLedgerAccount.AccountTitle.Should().Be(subLedgerAccount.AccountTitle);
        afterUpdateSubLedgerAccount.AccountCode.Should().NotBe(subLedgerAccount.AccountCode);
        afterUpdateSubLedgerAccount.JournalEntryType.Should().Be(subLedgerAccount.JournalEntryType);
        afterUpdateSubLedgerAccount.IsActive.Should().Be(subLedgerAccount.IsActive);
        afterUpdateSubLedgerAccount.GeneralLedgerAccountId.Should().NotBe(subLedgerAccount.GeneralLedgerAccountId);
    }

    [Fact]
    public async Task UpdateSubLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriod()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update subLedger account
        var updateSubLedgerAccountCommand = new UpdateSubLedgerAccountCommand
        {
            SubLedgerAccountId = subLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.SubLedgerAccountClient.UpdateAsync(updateSubLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot modify subLedger accounts without an active open financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task GetAllSubLedgerAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Get group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .ToListAsync();

        // Get all subLedger account
        var result = await setup.TestInit1.SubLedgerAccountClient.GetAllAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllSubLedgerAccounts_ShouldSucceed_WhenFilteredByTitle()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var createSubLedgerAccount = new CreateSubLedgerAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createSubLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountTitle == title)
            .ToListAsync();

        // Get all subLedger account
        var filter = new FilterSubLedgerAccountApiModel
        {
            Title = title
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.GetAllAsync(title: filter.Title);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllSubLedgerAccounts_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        var isActive = true;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var createSubLedgerAccount = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = isActive,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createSubLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.IsActive == isActive)
            .ToListAsync();

        // Get all subLedger account
        var filter = new FilterSubLedgerAccountApiModel
        {
            IsActive = isActive
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.GetAllAsync(isActive: filter.IsActive);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByJournalEntryType()
    {
        var journalEntryType = JournalEntryType.Credit;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var createSubLedgerAccount = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createSubLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.JournalEntryType == Domain.Enums.JournalEntryType.Credit)
            .ToListAsync();

        // Get all subLedger account
        var filter = new FilterSubLedgerAccountApiModel
        {
            JournalEntryType = (Domain.Enums.JournalEntryType?)journalEntryType
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.GetAllAsync(journalEntryType: (JournalEntryType?)filter.JournalEntryType);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByCode()
    {
        var code = "100";

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        
        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var createdAccount = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"100{Guid.NewGuid()}",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createdAccount);

        // Create subLedger account 2
        var createdAccount2 = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createdAccount2);

        // Create subLedger account 3
        var createdAccount3 = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(createdAccount3);

        // get all subLedger account
        var filter = new FilterSubLedgerAccountApiModel
        {
            Code = code
        };
        var result = await setup.TestInit1.SubLedgerAccountClient.GetAllAsync(code: filter.Code);

        // Get expected results from DB
        var expected = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountCode.Contains(code))
            .ToListAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(expected.Count);
        result.IsSuccess.Should().BeTrue();
    }
}