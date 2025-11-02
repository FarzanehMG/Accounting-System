using API.Controllers.Models.Accounts;
using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using CreateGeneralLedgerAccountApiModel = GenerateApi.CreateGeneralLedgerAccountApiModel;
using JournalEntryType = GenerateApi.JournalEntryType;

namespace APIIntegration.Tests.Accounts;

public class GeneralLedgerAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };
        await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created generalLedgerAccount via API: {model.Title}");

        // Get generalLedger account
        var createdGeneralAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Assert
        Assert.NotNull(createdGeneralAccount);
        Assert.Equal(model.Code, createdGeneralAccount.AccountCode);
        Assert.Equal(model.Title, createdGeneralAccount.AccountTitle);
        output.WriteLine($"Verified GeneralLedgerAccount exists in DB: {createdGeneralAccount.Id}");
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = generalLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("An active account with this title already exists in the selected group.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldSuccess_WhenInActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit, false);

        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();

    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = generalLedgerAccount.AccountCode,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateGeneralLedgerAccount_ShouldReturnInactiveGeneralLedgerAccountId_WhenInactiveGeneralLedgerWithSameTitleAndGroupAccountAndTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit, isActive: false);

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = generalLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {generalLedgerAccount.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{generalLedgerAccount.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldSuccess_WhenInactiveGeneralLedgerWithSameTitleAndTypeExistsInDifferentGroup()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = generalLedgerAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount2.Id
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().NotBeNull(result.Message);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenTitleIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = "",
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateGeneralLedgerAccount_ShouldFail_WhenJournalEntryTypeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = (JournalEntryType)3,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
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
        var model = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test generalLedger account",
            GroupAccountId = groupAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenGeneralLedgerAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = Guid.NewGuid()
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("GeneralLedger Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
        afterUpdateGeneralLedgerAccount.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSucceed_WhenTitleIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().NotBe(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenActiveGeneralLedgerAccountWithSameGroupAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount2.Id,
            Title = new PatchOfString
            {
                Value = generalLedgerAccount.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected group.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSucceed_WhenCodeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().NotBe(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSucceed_WhenJournalEntryTypeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // update generalLEdger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().NotBe(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenJournalEntriesExistAndJournalEntryTypeIsChanged()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccountCredit.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
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
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenValidatorFails()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = Guid.Empty,
            Title = new PatchOfString
            {
                Value = ""
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'General Ledger Account Id' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = Guid.NewGuid(),
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("GeneralLedger Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriod()
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

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot modify generalLedger accounts without an active open financial period.");
            ex.StatusCode.Should().Be(400);
        }

        financialPeriod.IsActive = true;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
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
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenCodeIsDuplicate()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = generalLedgerAccountDebit.AccountCode
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
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
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = generalLedgerAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected group.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create generalLedger account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit, isActive: false);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Title = new PatchOfString
            {
                Value = generalLedgerAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {generalLedgerAccount2.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{generalLedgerAccount2.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccountDebit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);
        var generalLedgerAccountDebit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id, generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccountCredit.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
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
    public async Task UpdateGeneralLedgerAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().NotBe(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeDescription_WhenProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountDescription.Should().NotBeNull();
        afterUpdateGeneralLedgerAccount.AccountDescription.Should().NotBe(generalLedgerAccount.AccountDescription);
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().Be(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldNotChangeDescription_WhenNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            Description = new PatchOfString
            {
                Value = null
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountDescription.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldFail_WhenGroupAccountInactive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccountCredit.Id,
            GroupAccountId = new PatchOfGuid
            {
                Value = Guid.NewGuid()
            }
        };

        // InActive group account
        groupAccount.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot assign account to an inactive or non-existent group.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldChangeGroupAccount_WhenActive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            GroupAccountId = new PatchOfGuid
            {
                Value = groupAccount2.Id
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountDescription.Should().Be(generalLedgerAccount.AccountDescription);
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().Be(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().NotBe(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var groupAccount2 = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Update generalLedger account
        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLedgerAccount.Id,
            GroupAccountId = new PatchOfGuid
            {
                Value = groupAccount2.Id
            },
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.UpdateAsync(updateGeneralLedgerAccountCommand);

        // Get generalLedger account
        var afterUpdateGeneralLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.Id == generalLedgerAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateGeneralLedgerAccount.AccountDescription.Should().Be(generalLedgerAccount.AccountDescription);
        afterUpdateGeneralLedgerAccount.AccountTitle.Should().Be(generalLedgerAccount.AccountTitle);
        afterUpdateGeneralLedgerAccount.AccountCode.Should().NotBe(generalLedgerAccount.AccountCode);
        afterUpdateGeneralLedgerAccount.JournalEntryType.Should().Be(generalLedgerAccount.JournalEntryType);
        afterUpdateGeneralLedgerAccount.IsActive.Should().Be(generalLedgerAccount.IsActive);
        afterUpdateGeneralLedgerAccount.GroupAccountId.Should().NotBe(generalLedgerAccount.GroupAccountId);
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Get group account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .AsNoTracking()
            .ToListAsync();

        // Get all generalLedger account
        var result = await setup.TestInit1.GeneralLedgerAccountClient.GetAllAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByTitle()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var createGeneralLedgerAccount = new CreateGeneralLedgerAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccount.Id
        };
        await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(createGeneralLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountTitle == title)
            .ToListAsync();

        // Get all generalLedger account
        var filter = new FilterGeneralLedgerAccountApiModel
        {
            Title = title
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.GetAllAsync(title: filter.Title);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllGeneralLedgerAccounts_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        var isActive = true;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var createGeneralLedgerAccount = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = isActive,
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccount.Id
        };
        await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(createGeneralLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.IsActive == isActive)
            .ToListAsync();

        // Get all generalLedger account
        var filter = new FilterGeneralLedgerAccountApiModel
        {
            IsActive = isActive
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.GetAllAsync(isActive: filter.IsActive);

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
        var createGeneralLedgerAccount = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccount.Id
        };
        await setup.TestInit1.GeneralLedgerAccountClient.CreateAsync(createGeneralLedgerAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<GeneralLedgerAccountEntity>()
            .AsNoTracking()
            .Where(x => x.JournalEntryType == Domain.Enums.JournalEntryType.Credit)
            .ToListAsync();

        // Get all generalLedger account
        var filter = new FilterGeneralLedgerAccountApiModel
        {
            JournalEntryType = (Domain.Enums.JournalEntryType?)journalEntryType
        };
        var result = await setup.TestInit1.GeneralLedgerAccountClient.GetAllAsync(journalEntryType: (JournalEntryType?)filter.JournalEntryType);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }
}