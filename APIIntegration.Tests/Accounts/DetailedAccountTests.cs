using API.Controllers.Models.Accounts;
using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using CreateDetailedAccountApiModel = GenerateApi.CreateDetailedAccountApiModel;

namespace APIIntegration.Tests.Accounts;

public class DetailedAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateDetailedAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created detailedAccount via API: {model.Title}");

        // Verify in database
        var createdDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        // Assert
        Assert.NotNull(createdDetailedAccount);
        Assert.Equal(model.Code, createdDetailedAccount.AccountCode);
        Assert.Equal(model.Title, createdDetailedAccount.AccountTitle);
        output.WriteLine($"Verified DetailedAccount exists in DB: {createdDetailedAccount.Id}");
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenSubLedgerNotFound()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = Guid.NewGuid()
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("SubLedger not found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenJournalEntryTypeNotSameWithSubLedgerAccount()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Debit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Debit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Debit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("Detailed account must have the same nature (JournalEntryType) as its SubLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenSubLedgerAccountInActive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("The SubLedger account must be active.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenActiveDetailedAccountWithSameSubLedgerAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = detailedAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain("An active account with this title already exists in the selected subLedger.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldSuccess_WhenInActiveDetailedAccountWithSameSubLedgerAccountAndJournalEntryTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        // Create detailed account
        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };
        var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = detailedAccount.AccountCode,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateDetailedAccount_ShouldReturnInactiveDetailedAccountId_WhenInactiveDetailedWithSameTitleAndSubLedgerAccountAndTypeExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create Detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateDetailedAccountApiModel
        {
            Title = detailedAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Message;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {detailedAccount.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{detailedAccount.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldSuccess_WhenInactiveDetailedWithSameTitleAndTypeExistsInDifferentSubLedgerAccount()
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

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateDetailedAccountApiModel
        {
            Title = detailedAccount.AccountTitle,
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount2.Id
        };
        var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().NotBeNull(result.Message);
    }

    [Fact]
    public async Task CreateDetailedAccount_ShouldFail_WhenTitleIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = "",
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateDetailedAccount_ShouldFail_WhenJournalEntryTypeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = (JournalEntryType)3,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task CreateDetailedAccount_ShouldFail_WhenCodeIsNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
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

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Create SubLedger account
        var model = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test detailed account",
            SubLedgerAccountId = subLedgerAccount.Id
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.CreateAsync(model, CancellationToken.None);
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
    public async Task UpdateDetailedAccount_ShouldFail_WhenDetailedAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = Guid.NewGuid()
        };

        // Act and Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Detailed Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldNotChange_WhenNoFieldsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
        afterUpdateDetailedAccount.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldSucceed_WhenTitleIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountTitle.Should().NotBe(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenActiveDetailedAccountWithSameSubLedgerAccountAndJournalEntryTypeAndTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);
        var detailedAccount2 = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount2.Id,
            Title = new PatchOfString
            {
                Value = detailedAccount.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected detailed account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldSucceed_WhenCodeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().NotBe(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldSucceed_WhenJournalEntryTypeIsProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().NotBe(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenJournalEntriesExistAndJournalEntryTypeIsChanged()
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

        // Create detailed account
        var detailedAccountCredit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountCredit.Id, JournalEntryType.Credit);
        var detailedAccountDebit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id,
            generalLedgerAccountCredit.Id,
            generalLedgerAccountDebit.Id,
            subLedgerAccountCredit,
            subLedgerAccountDebit,
            detailedAccountCredit,
            detailedAccountDebit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountCredit.Id,
            JournalEntryType = new PatchOfJournalEntryType
            {
                Value = JournalEntryType.Debit
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
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
    public async Task UpdateDetailedAccount_ShouldFail_WhenValidatorFails()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = Guid.Empty,
            Title = new PatchOfString
            {
                Value = ""
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("'Detailed Account Id' must not be empty.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = Guid.NewGuid(),
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Detailed Account Not Found.");
            ex.StatusCode.Should().Be(404);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenCodeChangedAndHasJournalEntries()
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

        // Create subLedger account
        var detailedAccountCredit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountCredit.Id, JournalEntryType.Credit);
        var detailedAccountDebit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id,
            generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id,
            subLedgerAccountCredit, subLedgerAccountDebit,
            detailedAccountCredit, detailedAccountDebit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
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
    public async Task UpdateDetailedAccount_ShouldFail_WhenCodeIsDuplicate()
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

        // Create detailed account
        var detailedAccountCredit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountCredit.Id, JournalEntryType.Credit);
        var detailedAccountDebit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountCredit.Id,
            Code = new PatchOfString
            {
                Value = detailedAccountDebit.AccountCode
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
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
    public async Task UpdateDetailedAccount_ShouldFail_WhenActiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);
        var detailedAccount2 = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Title = new PatchOfString
            {
                Value = detailedAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("An active account with this title already exists in the selected detailed account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldSuggestReactivation_WhenInactiveDuplicateTitleExists()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccountCredit = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccountCredit = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccountCredit.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccountCredit.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);
        var detailedAccount2 = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Title = new PatchOfString
            {
                Value = detailedAccount2.AccountTitle
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain($"An inactive account with this title already exists (Code: {detailedAccount2.AccountCode}). Consider reactivating it instead of creating a new one.");
            responseContent.Should().Contain($"{detailedAccount2.Id}");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenDeactivatingWithJournalEntries()
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

        // Create subLedger account
        var detailedAccountCredit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountCredit.Id, JournalEntryType.Credit);
        var detailedAccountDebit = await setup.TestInit1.CreateDetailedAccount(subLedgerAccountDebit.Id, JournalEntryType.Debit);

        // Create journal entry
        await setup.TestInit1.CreateJournalEntry(financialPeriod.Id, groupAccountCredit.Id, groupAccountDebit.Id,
            generalLedgerAccountCredit.Id, generalLedgerAccountDebit.Id,
            subLedgerAccountCredit, subLedgerAccountDebit,
            detailedAccountCredit, detailedAccountDebit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountCredit.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
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
    public async Task UpdateDetailedAccount_ShouldChangeIsActive_WhenNoJournalEntries()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            IsActive = new PatchOfBoolean
            {
                Value = false
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().NotBe(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldChangeDescription_WhenProvided()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Description = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountDescription.Should().NotBeNull();
        afterUpdateDetailedAccount.AccountDescription.Should().NotBe(detailedAccount.AccountDescription);
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().Be(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldNotChangeDescription_WhenNull()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Description = new PatchOfString
            {
                Value = null
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountDescription.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenSubLedgerAccountInactive()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            SubLedgerAccountId = new PatchOfGuid
            {
                Value = Guid.NewGuid()
            }
        };

        // InActive subLedger account
        subLedgerAccount.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot assign account to an inactive or non-existent subLedger account.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldChangeSubLedgerAccount_WhenActive()
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

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            SubLedgerAccountId = new PatchOfGuid
            {
                Value = subLedgerAccount2.Id
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountDescription.Should().Be(detailedAccount.AccountDescription);
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().Be(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().NotBe(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldUpdateAllFields_WhenMultipleUpdatesProvided_NoJournalEntries()
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

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            SubLedgerAccountId = new PatchOfGuid
            {
                Value = subLedgerAccount2.Id
            },
            Code = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };
        var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);

        // Get detailed account
        var afterUpdateDetailedAccount = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == detailedAccount.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        afterUpdateDetailedAccount.AccountDescription.Should().Be(detailedAccount.AccountDescription);
        afterUpdateDetailedAccount.AccountTitle.Should().Be(detailedAccount.AccountTitle);
        afterUpdateDetailedAccount.AccountCode.Should().NotBe(detailedAccount.AccountCode);
        afterUpdateDetailedAccount.JournalEntryType.Should().Be(detailedAccount.JournalEntryType);
        afterUpdateDetailedAccount.IsActive.Should().Be(detailedAccount.IsActive);
        afterUpdateDetailedAccount.SubLedgerAccountId.Should().NotBe(detailedAccount.SubLedgerAccountId);
    }

    [Fact]
    public async Task UpdateDetailedAccount_ShouldFail_WhenNoActiveFinancialPeriod()
    {
        // Create financial period
        var financialPeriod = await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var detailedAccount = await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Update financial period
        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

        // Update detailed account
        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccount.Id,
            Title = new PatchOfString
            {
                Value = Guid.NewGuid().ToString()
            }
        };

        // Act And Assert
        try
        {
            var result = await setup.TestInit1.DetailedAccountClient.UpdateAsync(updateDetailedAccountCommand);
            result.IsSuccess.Should().BeFalse("Expected an ApiException was not thrown.");
        }
        catch (ApiException ex)
        {
            var responseContent = ex.Response;
            responseContent.Should().Contain("Cannot modify detailed accounts without an active open financial period.");
            ex.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task GetAllDetailedAccounts_ShouldSucceed_WhenNoFiltersApplied()
    {
        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        await setup.TestInit1.CreateDetailedAccount(subLedgerAccount.Id, JournalEntryType.Credit);

        // Get detailed account
        var original = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .ToListAsync();

        // Get all detailed account
        var result = await setup.TestInit1.DetailedAccountClient.GetAllAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllDetailedAccounts_ShouldSucceed_WhenFilteredByTitle()
    {
        var title = Guid.NewGuid().ToString();

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var createDetailedAccount = new CreateDetailedAccountApiModel
        {
            Title = title,
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createDetailedAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountTitle == title)
            .ToListAsync();

        // Get all detailed account
        var filter = new FilterDetailedAccountApiModel
        {
            Title = title
        };
        var result = await setup.TestInit1.DetailedAccountClient.GetAllAsync(title: filter.Title);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllDetailedAccounts_ShouldSucceed_WhenFilteredByActiveFlag()
    {
        var isActive = true;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var createDetailedAccount = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = isActive,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createDetailedAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .Where(x => x.IsActive == isActive)
            .ToListAsync();

        // Get all detailed account
        var filter = new FilterDetailedAccountApiModel
        {
            IsActive = isActive
        };
        var result = await setup.TestInit1.DetailedAccountClient.GetAllAsync(isActive: filter.IsActive);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllDetailedAccounts_ShouldSucceed_WhenFilteredByJournalEntryType()
    {
        var journalEntryType = JournalEntryType.Credit;

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var createDetailedAccount = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createDetailedAccount);

        // Get expected results from DB
        var original = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .Where(x => x.JournalEntryType == Domain.Enums.JournalEntryType.Credit)
            .ToListAsync();

        // Get all detailed account
        var filter = new FilterDetailedAccountApiModel
        {
            JournalEntryType = (Domain.Enums.JournalEntryType?)journalEntryType
        };
        var result = await setup.TestInit1.DetailedAccountClient.GetAllAsync(journalEntryType: (JournalEntryType?)filter.JournalEntryType);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(original.Count);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllDetailedAccounts_ShouldSucceed_WhenFilteredByCode()
    {
        var code = "100";

        // Create financial period
        await setup.TestInit1.GetOrCreateFinancialPeriod();

        // Create group account
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);

        // Create generalLedger account
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        // Create subLedger account
        var subLedgerAccount = await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);

        // Create detailed account
        var createdAccount = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"100{Guid.NewGuid()}",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createdAccount);

        // Create detailed account 2
        var createdAccount2 = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = $"{Guid.NewGuid()}1001",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createdAccount2);

        // Create detailed account 3
        var createdAccount3 = new CreateDetailedAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test",
            SubLedgerAccountId = subLedgerAccount.Id
        };
        await setup.TestInit1.DetailedAccountClient.CreateAsync(createdAccount3);

        // get all detailed account
        var filter = new FilterDetailedAccountApiModel
        {
            Code = code
        };
        var result = await setup.TestInit1.DetailedAccountClient.GetAllAsync(code: filter.Code);

        // Get expected results from DB
        var expected = await setup.TestInit1.AccountingDbContext
            .Set<DetailedAccountEntity>()
            .AsNoTracking()
            .Where(x => x.AccountCode.Contains(code))
            .ToListAsync();

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(expected.Count);
        result.IsSuccess.Should().BeTrue();
    }
}