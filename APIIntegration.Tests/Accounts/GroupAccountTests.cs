using APIIntegration.Tests.Helper;
using Domain.Entities.Accounts;
using GenerateApi;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace APIIntegration.Tests.Accounts;

public class GroupAccountTests(ApiTestSetup setup, ITestOutputHelper output) : IClassFixture<ApiTestSetup>
{
    [Fact]
    public async Task CreateGroupAccount_Should_Create_Successfully()
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

}