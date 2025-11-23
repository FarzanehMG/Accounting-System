using API.Controllers.Models.Accounts;
using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using CreateGroupAccountApiModel = GenerateApi.CreateGroupAccountApiModel;
using UpdateGroupAccountCommand = GenerateApi.UpdateGroupAccountCommand;

namespace APIIntegration.Tests.Accounts;

public class GroupAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // clean DB before each test
        await setup.TestInit1.DeleteFinancialPeriodAndDependenciesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created GroupAccount via API: {model.Title}");

        // Get group account
        var createdGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Assert
        Assert.NotNull(createdGroupAccount);
        Assert.Equal(model.Code, createdGroupAccount.AccountCode);
        Assert.Equal(model.Title, createdGroupAccount.AccountTitle);
        output.WriteLine($"Verified GroupAccount exists in DB: {createdGroupAccount.Id}");
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenActiveGroupAccountWithSameJournalEntryTypeExists()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };
        var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
        result.IsSuccess.Should().BeTrue();

        // Create group account 2
        var createGroupAccountCommand2 = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result2 = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand2);
            result2.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("An active group account with this title already exists.");
        }
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldSucceed_WhenInactiveGroupAccountWithSameTitleExists()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };
        var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);

        // Create group account 2
        var createGroupAccountCommand2 = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = false,
            JournalEntryType = JournalEntryType.Debit,
            Description = "Integration test group account"
        };
        var result2 = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenTitleIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = "",
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
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
    public async Task CreateGroupAccount_ShouldFail_WhenJournalEntryTypeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = (JournalEntryType)3,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
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
    public async Task CreateGroupAccount_ShouldFail_WhenCodeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = "",
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
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
    public async Task CreateGroupAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = groupAccount.AccountCode,
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Group account code already exists.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create group account
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("No active financial period found.");
            ex.StatusCode.Should().Be(400);
        }

        financialPeriod.IsActive = true;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldReturnInactiveGroupAccountId_WhenInactiveGroupWithSameTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit, false);

        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = groupAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain($"An inactive group account with this title already exists (Code: {groupAccount.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{groupAccount.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Get created group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = original.Id
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get updated group account
        var updated = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == original.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        updated.AccountTitle.Should().Be(original.AccountTitle);
        updated.AccountCode.Should().Be(original.AccountCode);
        updated.AccountDescription.Should().Be(original.AccountDescription);
        updated.JournalEntryType.Should().Be(original.JournalEntryType);
        updated.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenGroupAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = Guid.NewGuid()
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Group Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldSucceed_WhenTitleIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Get created group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = original.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get updated group account
        var updated = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == original.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        updated.AccountTitle.Should().NotBe(original.AccountTitle);
        updated.AccountCode.Should().Be(original.AccountCode);
        updated.AccountDescription.Should().Be(original.AccountDescription);
        updated.JournalEntryType.Should().Be(original.JournalEntryType);
        updated.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeCode_WhenValidAndNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Get created group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = original.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get updated group account
        var updated = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == original.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        updated.AccountTitle.Should().Be(original.AccountTitle);
        updated.AccountCode.Should().NotBe(original.AccountCode);
        updated.AccountDescription.Should().Be(original.AccountDescription);
        updated.JournalEntryType.Should().Be(original.JournalEntryType);
        updated.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeJournalType_WhenNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Get created group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = original.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get updated group account
        var updated = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == original.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        updated.AccountTitle.Should().Be(original.AccountTitle);
        updated.AccountCode.Should().Be(original.AccountCode);
        updated.AccountDescription.Should().Be(original.AccountDescription);
        updated.JournalEntryType.Should().NotBe(original.JournalEntryType);
        updated.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenJournalEntriesExistAndJournalEntryTypeIsChanged()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = creditGroupAccount.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };

        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot change Journal Entry Type for a used account");
        }

        // Get updated group account
        var updated = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(g => g.Id == creditGroupAccount.Id);

        // Assert
        updated.AccountTitle.Should().Be(creditGroupAccount.AccountTitle);
        updated.AccountCode.Should().Be(creditGroupAccount.AccountCode);
        updated.AccountDescription.Should().Be(creditGroupAccount.AccountDescription);
        updated.JournalEntryType.Should().Be(creditGroupAccount.JournalEntryType);
        updated.IsActive.Should().Be(creditGroupAccount.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenValidatorFails()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            Title = new PatchOfString
            {
                Value = ""
            }
        };

        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Title cannot be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenNoActiveFinancialPeriod()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot modify group accounts without an active open financial period.");
            ex.StatusCode.Should().Be(400);
        }

        financialPeriod.IsActive = true;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = creditGroupAccount.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot change account code for a used account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenCodeIsDuplicate()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount2.Id,
            Code = new PatchOfString
            {
                Value = groupAccount.AccountCode
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Group account code already exists. Please provide a unique code.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount2.Id,
            Title = new PatchOfString
            {
                Value = groupAccount.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active group account with this title already exists. Consider renaming or reactivating the existing account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit, isActive:false);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount2.Id,
            Title = new PatchOfString
            {
                Value = groupAccount.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain($"An inactive group account with this title already exists (Code: {groupAccount.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{groupAccount.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var creditGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var debitGroupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(creditGroupAccount.Id, debitGroupAccount.Id, financialPeriod);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = creditGroupAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot deactivate a group account with existing journal entries.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };

        // Act
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get group account
        var getGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .SingleAsync(f => f.Id == groupAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        getGroupAccount.IsActive.Should().BeTrue();

    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeDescription_WhenProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get group account
        var getGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(f => f.Id == groupAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        getGroupAccount.AccountDescription.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldNotChangeDescription_WhenNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            Description = new PatchOfString
            {
                Value = null
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get group account
        var getGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(f => f.Id == groupAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        getGroupAccount.AccountDescription.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            },
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);

        // Get group account
        var getGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .SingleAsync(f => f.Id == groupAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        getGroupAccount.IsActive.Should().BeFalse();
        getGroupAccount.AccountCode.Should().NotBe(groupAccount.AccountCode);
    }

    [Fact]
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createdAccount = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount);

        // Get created group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .ToListAsync();

        // Get all group account
        var result = await setup.TestInit1.GroupAccountClient.GetAllAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenFilteredByTitle()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createdAccount = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount);

        // Get expected results from DB
        var expected = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountTitle == title)
            .ToListAsync();

        // Get all group account
        var filter = new FilterGroupAccountApiModel
        {
            Title = title
        };
        var result = await setup.TestInit1.GroupAccountClient.GetAllAsync(title:filter.Title);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(expected.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenFilteredByCode()
    {
        var code = "100";

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createdAccount = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"100{Guid.NewGuid()}",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount);

        // Create group account 2
        var createdAccount2 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount2);

        // Create group account 3
        var createdAccount3 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount3);

        // get all group account
        var filter = new FilterGroupAccountApiModel
        {
            Code = code
        };
        var result = await setup.TestInit1.GroupAccountClient.GetAllAsync(code:filter.Code);

        // Get expected results from DB
        var expected = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountCode.Contains(code))
            .ToListAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(expected.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenFilteredByJournalEntryType()
    {
        var journalEntryType = JournalEntryType.Credit;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var createdAccount = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"100{Guid.NewGuid()}",
            JournalEntryType = journalEntryType,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount);

        // Create group account 2
        var createdAccount2 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = journalEntryType,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount2);

        // Create group account 3
        var createdAccount3 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount3);

        // Get all group account
        var result = await setup.TestInit1.GroupAccountClient.GetAllAsync(journalEntryType: journalEntryType);

        // Get expected results from DB
        var expected = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .AsNoTracking()
            .Where(x => x.JournalEntryType == Domain.Enums.JournalEntryType.Credit)
            .ToListAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.All(x => x.JournalEntryType == journalEntryType).Should().BeTrue();
        result.Data.Count.Should().Be(expected.Count);
        result.IsSuccess.Should().BeTrue();
    }
}