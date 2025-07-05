using Application.Common;
using Application.Features.Accounts.Commands.GroupAccount;
using Application.Features.Accounts.Queries.GroupAccount;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;
using GetGroupAccountByTitleQueryHandler = Application.Features.Accounts.Queries.GroupAccount.GetGroupAccountByTitleQueryHandler;

namespace Application.Tests;

public class GroupAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Creating_New_GroupAccount_Should_Be_Success()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var accountCodeGenerator = Substitute.For<IAccountCodeGenerator>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand(faker.Finance.AccountName(),
            null,
            JournalEntryType.Credit,
            true);
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork, accountCodeGenerator);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.GroupAccountRepository.Received(1)
            .CreateAsync(Arg.Any<GroupAccountEntity>(), CancellationToken.None);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GroupAccount_Title_Should_Be_Valid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var accountCodeGenerator = Substitute.For<IAccountCodeGenerator>();

        var createGroupAccountCommand = new CreateGroupAccountCommand("",
            null,
            JournalEntryType.Credit,
            true);
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork, accountCodeGenerator);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task GroupAccount_JournalEntryType_Should_Be_Valid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var accountCodeGenerator = Substitute.For<IAccountCodeGenerator>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand(faker.Finance.AccountName(),
            null,
            0,
            true);
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork, accountCodeGenerator);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task GroupAccount_Should_Use_Code_Generated_By_CodeGenerator()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var accountCodeGenerator = Substitute.For<IAccountCodeGenerator>();
        var faker = new Faker();
        const string expectedCode = "101";
        accountCodeGenerator.GenerateCode().Returns(expectedCode);

        var command = new CreateGroupAccountCommand(faker.Finance.AccountName(), "Description", JournalEntryType.Credit, true);
        var handler = new CreateGroupAccountCommandHandler(unitOfWork, accountCodeGenerator);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.GroupAccountRepository.Received(1).CreateAsync(
            Arg.Is<GroupAccountEntity>(x => x.AccountCode == expectedCode), CancellationToken.None);
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var accountCodeGenerator = Substitute.For<IAccountCodeGenerator>();
        var command = new CreateGroupAccountCommand("", null, JournalEntryType.Credit, true);
        var handler = new CreateGroupAccountCommandHandler(unitOfWork, accountCodeGenerator);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive().CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Update_GroupAccount_Title()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .GetByIdAsync(existingGroupAccount.Id, CancellationToken.None)
            .Returns(existingGroupAccount);

        var updateGroupAccountTitle = new UpdateGroupAccountCommand
        {
            GroupAccountId = existingGroupAccount.Id,
            Title = new Patch<string?>(faker.Finance.AccountName())
        };
        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountTitle, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGroupAccount.AccountTitle.Should().Be(updateGroupAccountTitle.Title!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Update_GroupAccount_IsActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountDescription = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .GetByIdAsync(existingGroupAccount.Id, CancellationToken.None)
            .Returns(existingGroupAccount);

        var updateGroupAccountIsActive = new UpdateGroupAccountCommand
        {
            GroupAccountId = existingGroupAccount.Id,
            IsActive = new Patch<bool>(false)
        };

        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountIsActive, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGroupAccount.IsActive.Should().Be(updateGroupAccountIsActive.IsActive!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Update_GroupAccount_Description()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountDescription = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .GetByIdAsync(existingGroupAccount.Id, CancellationToken.None)
            .Returns(existingGroupAccount);

        var updateGroupAccountDescription = new UpdateGroupAccountCommand
        {
            GroupAccountId = existingGroupAccount.Id,
            Description = new Patch<string?>(faker.Finance.AccountName())
        };

        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountDescription, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGroupAccount.AccountDescription.Should().Be(updateGroupAccountDescription.Description!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Update_GroupAccount_JournalEntryType()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountDescription = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .GetByIdAsync(existingGroupAccount.Id, CancellationToken.None)
            .Returns(existingGroupAccount);

        var updateGroupAccountJournalEntryType = new UpdateGroupAccountCommand
        {
            GroupAccountId = existingGroupAccount.Id,
            JournalEntryType = new Patch<JournalEntryType>(JournalEntryType.Credit)
        };

        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountJournalEntryType, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGroupAccount.JournalEntryType.Should().Be(updateGroupAccountJournalEntryType.JournalEntryType!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Should_Return_GroupAccount_When_Id_Is_Valid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };

        unitOfWork.GroupAccountRepository
            .GetByIdAsync(existingGroupAccount.Id, Arg.Any<CancellationToken>())
            .Returns(existingGroupAccount);

        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery(existingGroupAccount.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Id.Should().Be(existingGroupAccount.Id);
        result.Result.AccountTitle.Should().Be(existingGroupAccount.AccountTitle);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_GroupAccount_Does_Not_Exist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        unitOfWork.GroupAccountRepository
            .GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((GroupAccountEntity?)null);

        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(
            new KeyValuePair<string, string>(nameof(GetGroupAccountByIdQuery.Id), "Group account not found."));
    }

    [Fact]
    public async Task Should_Fail_When_Validation_Fails()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery(Guid.Empty), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetGroupAccountByIdQuery.Id) && x.Value.Contains("must not be empty"));
    }

    [Fact]
    public async Task Should_Return_GroupAccounts_When_Title_Is_Valid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        var existingGroupAccounts = new List<GroupAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit
            }
        };

        unitOfWork.GroupAccountRepository
            .GetByTitleAsync(title, CancellationToken.None)
            .Returns(existingGroupAccounts);

        var query = new GetGroupAccountByTitleQuery(title);
        var handler = new GetGroupAccountByTitleQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(title);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Title_Has_No_Results()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        unitOfWork.GroupAccountRepository
            .GetByTitleAsync(title, CancellationToken.None)
            .Returns([]);

        var query = new GetGroupAccountByTitleQuery(title);
        var handler = new GetGroupAccountByTitleQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(
            new KeyValuePair<string, string>(nameof(GetGroupAccountByTitleQueryResult.AccountTitle), "Group account not found.")
        );
    }

    [Fact]
    public async Task Should_Fail_When_Title_Is_Empty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var query = new GetGroupAccountByTitleQuery("");
        var handler = new GetGroupAccountByTitleQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetGroupAccountByTitleQuery.AccountTitle) && x.Value.Contains("must not be empty"));
    }
}