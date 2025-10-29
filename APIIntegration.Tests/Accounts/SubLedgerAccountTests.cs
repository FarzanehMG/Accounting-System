using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using Domain.Entities.FinancialPeriod;
using FluentAssertions;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace APIIntegration.Tests.Accounts;

public class SubLedgerAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateSubLedgerAccount_ShouldSuccess_WhenRequestIsValid()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act
        await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert API response
        output.WriteLine($"Created subLedgerAccount via API: {model.Title}");

        // Verify in database
        var createdSubLedgerAccount = await setup.TestInit1.AccountingDbContext
            .Set<SubLedgerAccountEntity>()
            .SingleOrDefaultAsync(g => g.AccountCode == model.Code && g.AccountTitle == model.Title);

        Assert.NotNull(createdSubLedgerAccount);
        Assert.Equal(model.Code, createdSubLedgerAccount.AccountCode);
        Assert.Equal(model.Title, createdSubLedgerAccount.AccountTitle);
        output.WriteLine($"Verified SubLedgerAccount exists in DB: {createdSubLedgerAccount.Id}");
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenGeneralLedgerNotFound()
    {
        // Arrange
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        await setup.TestInit1.CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit, isActive: false);

        var model = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = "Integration test subLedger account",
            GeneralLedgerAccountId = generalLedgerAccount.Id
        };

        // Act
        var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenDuplicateCodeExists()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
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
    public async Task CreateSubLedgerAccount_ShouldFail_WhenNoActiveFinancialPeriodExists()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);

        var financialPeriod = await setup.TestInit1.AccountingDbContext
            .Set<FinancialPeriodEntity>()
            .SingleAsync(f => f.Id == setup.TestInit1.FinancialPeriodId);

        financialPeriod.IsActive = false;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();

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

        financialPeriod.IsActive = true;
        await setup.TestInit1.AccountingDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldReturnInactiveSubLedgerAccountId_WhenInactiveSubLedgerWithSameTitleAndGeneralLedgerAccountAndTypeExists()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
        var generalLedgerAccount = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
        var generalLedgerAccount2 = await setup.TestInit1.CreateGeneralLedgerAccount(groupAccount.Id, JournalEntryType.Credit);
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

        // Act
        var result = await setup.TestInit1.SubLedgerAccountClient.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().NotBeNull(result.Message);
    }

    [Fact]
    public async Task CreateSubLedgerAccount_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
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
        // Arrange
        var groupAccount = await setup.TestInit1.CreateGroupAccount(JournalEntryType.Credit);
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
}