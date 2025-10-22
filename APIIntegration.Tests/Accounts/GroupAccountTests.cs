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

public class GroupAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateGroupAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Arrange
        var model = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test group account"
        };

        // Act
        await setup.TestInit1.GroupAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created GroupAccount via API: {model.Title}");

        // Verify in database
        var createdGroupAccount = await setup.TestInit1.AccountingDbContext
            .Set<GroupAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        Assert.NotNull(createdGroupAccount);
        Assert.Equal(model.Code, createdGroupAccount.AccountCode);
        Assert.Equal(model.Title, createdGroupAccount.AccountTitle);
        output.WriteLine($"Verified GroupAccount exists in DB: {createdGroupAccount.Id}");
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenActiveGroupAccountWithSameJournalEntryTypeExists()
    {
        // Arrange
        var title = Guid.NewGuid().ToString();

        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        var createGroupAccountCommand2 = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        // Act and Assert
        var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
        result.IsSuccess.Should().BeTrue();

        // Act and Assert
        try
        {
            var result2 = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand2);
            result2.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("An active group account with the same title or type already exists.");
        }
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldSucceed_WhenInactiveGroupAccountWithSameTitleExists()
    {
        // Arrange
        var title = Guid.NewGuid().ToString();

        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            Description = "Integration test group account"
        };

        var createGroupAccountCommand2 = new CreateGroupAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = false,
            JournalEntryType = JournalEntryType.Debit,
            Description = "Integration test group account"
        };

        // Act
        var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
        var result2 = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
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
            result.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
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
        // Arrange
        var createGroupAccountCommand = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = default,
            Description = "Integration test group account"
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.CreateAsync(createGroupAccountCommand);
            result.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'JournalEntryType' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenCodeIsNull()
    {
        // Arrange
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
            result.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
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
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldReturnInactiveGroupAccountId_WhenInactiveGroupWithSameTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateGroupAccount_ShouldSuccess_WhenFinancialPeriodIsActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
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
        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GroupAccountClient.GetByIdAsync(Guid.NewGuid());
            result.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Group account not found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldSucceed_WhenTitleIsProvided()
    {
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

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(original.Id);

        // Update group account
        var updateGroupAccountCommand = new UpdateGroupAccountCommand
        {
            GroupAccountId = original.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };

        try
        {
            var result = await setup.TestInit1.GroupAccountClient.UpdateAsync(updateGroupAccountCommand);
            result.IsSuccess.Should().BeTrue("Expected an ApiException was not thrown.");
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
            .SingleAsync(g => g.Id == original.Id);

        // Assert
        updated.AccountTitle.Should().Be(original.AccountTitle);
        updated.AccountCode.Should().Be(original.AccountCode);
        updated.AccountDescription.Should().Be(original.AccountDescription);
        updated.JournalEntryType.Should().Be(original.JournalEntryType);
        updated.IsActive.Should().Be(original.IsActive);
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenValidatorFails()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenNoActiveFinancialPeriod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenFinancialPeriodInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenCodeIsDuplicate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeIsActive_WhenActivatingAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldChangeDescription_WhenProvided()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldNotChangeDescription_WhenNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldFailOrChangeFields_WhenMultipleUpdatesProvided_WithJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateGroupAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
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

        // Act
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

        // Act
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
    public async Task GetAllGroupAccounts_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        var code = "100";

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

        var createdAccount2 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount2);

        var createdAccount3 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount3);

        // Act
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

        var createdAccount2 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = journalEntryType,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount2);

        var createdAccount3 = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true,
            Description = "Integration test"
        };
        await setup.TestInit1.GroupAccountClient.CreateAsync(createdAccount3);

        // Act
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