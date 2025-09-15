using Application.Common;
using Application.Features.Accounts.Commands.GroupAccount;
using Application.Features.Accounts.Queries.GroupAccount;
using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Accounts;

public class GroupAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateGroupAccountHandler_WhenGroupAccountCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

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
    public async Task CreateGroupAccountHandler_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = "",
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateGroupAccountHandler_ShouldFail_WhenJournalEntryTypeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            JournalEntryType = 0,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateGroupAccountHandler_ShouldFail_WhenCodeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

        // Act
        await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive().CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateGroupAccountHandler_ShouldFail_WhenExistSameActiveTitle()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .FindActiveAccountByTitleAndTypeAsync(title, JournalEntryType.Debit, Arg.Any<CancellationToken>())
            .Returns(existingGroupAccount);

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGroupAccountCommandHandler.Handle(createGroupAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(createGroupAccountCommand.Title))
            .WhoseValue.Should().Contain("An active group account with the same title or type already exists.");

        await unitOfWork.GroupAccountRepository.DidNotReceive()
            .CreateAsync(Arg.Any<GroupAccountEntity>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateGroupAccountHandler_ShouldSuccess_WhenExistSameActiveTitleWithDifferentJournalEntryType()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        var existingGroupAccount = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        };
        unitOfWork.GroupAccountRepository
            .FindActiveAccountByTitleAndTypeAsync(title, JournalEntryType.Debit, Arg.Any<CancellationToken>())
            .Returns(existingGroupAccount);

        var createGroupAccountCommand = new CreateGroupAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true
        };
        var createGroupAccountCommandHandler = new CreateGroupAccountCommandHandler(unitOfWork);

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
    public async Task UpdateGroupAccountHandler_TitleProvided_UpdatesTitle()
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
            Title = new Patch<string>(faker.Finance.AccountName())
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
    public async Task UpdateGroupAccountHandler_IsActiveProvided_UpdatesIsActive()
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
    public async Task UpdateGroupAccountHandler_DescriptionProvided_UpdatesDescription()
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
            Description = new Patch<string>(faker.Finance.AccountName())
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
    public async Task UpdateGroupAccountHandler_JournalEntryTypeProvided_UpdatesJournalEntryType()
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
    public async Task UpdateGroupAccountHandler_CodeProvided_UpdatesCode()
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

        var updateGroupAccountCode = new UpdateGroupAccountCommand
        {
            GroupAccountId = existingGroupAccount.Id,
            Code = new Patch<string>(faker.Finance.AccountName())
        };

        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountCode, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGroupAccount.AccountCode.Should().Be(updateGroupAccountCode.Code!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGroupAccountHandler_WhenGroupAccountNotFound_ShouldReturnNotFoundResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var groupAccountId = Guid.NewGuid();

        unitOfWork.GroupAccountRepository
            .GetByIdAsync(groupAccountId, Arg.Any<CancellationToken>())
            .Returns((GroupAccountEntity?)null);

        var updateGroupAccountCode = new UpdateGroupAccountCommand
        {
            GroupAccountId = groupAccountId,
            Code = new Patch<string>(faker.Finance.AccountName())
        };

        var handler = new UpdateGroupAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateGroupAccountCode, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGroupAccountCode.GroupAccountId))
            .WhoseValue.Should().Contain("Group Account Not Found.");
    }

    [Fact]
    public async Task GetGroupAccount_WithValidId_ShouldReturnAccount()
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

        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery
        {
            Id = existingGroupAccount.Id
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Id.Should().Be(existingGroupAccount.Id);
        result.Result.AccountTitle.Should().Be(existingGroupAccount.AccountTitle);
    }

    [Fact]
    public async Task GetGroupAccount_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        unitOfWork.GroupAccountRepository
            .GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((GroupAccountEntity?)null);

        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery
        {
            Id = Guid.NewGuid()
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(new KeyValuePair<string, string>(nameof(GetGroupAccountByIdQuery.Id), "Group account not found."));
    }

    [Fact]
    public async Task GetGroupAccount_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetGroupAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGroupAccountByIdQuery
        {
            Id = Guid.Empty
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetGroupAccountByIdQuery.Id) && x.Value.Contains("must not be empty"));
    }

    [Fact]
    public async Task GetGroupAccounts_WhenTitleIsValid_ShouldReturnMatchingResults()
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
            .GetAllAsync(new GroupAccountFilter
            {
                Title = title
            }, CancellationToken.None)
            .Returns(existingGroupAccounts);

        var query = new GetGroupAccountsQuery
        {
            Title = title
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(title);
    }

    [Fact]
    public async Task GetGroupAccounts_WhenCodeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var code = faker.Finance.AccountName();

        var existingGroupAccounts = new List<GroupAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit
            }
        };

        unitOfWork.GroupAccountRepository
            .GetAllAsync(new GroupAccountFilter
            {
                Code = code
            }, CancellationToken.None)
            .Returns(existingGroupAccounts);

        var query = new GetGroupAccountsQuery
        {
            Code = code
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountCode).Distinct().Should().ContainSingle().Which.Should().Be(code);
    }

    [Fact]
    public async Task GetGroupAccounts_WhenIsActiveIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const bool isActive = true;

        var existingGroupAccounts = new List<GroupAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = isActive,
                JournalEntryType = JournalEntryType.Debit
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = isActive,
                JournalEntryType = JournalEntryType.Credit
            }
        };

        unitOfWork.GroupAccountRepository
            .GetAllAsync(new GroupAccountFilter
            {
                IsActive = true
            }, CancellationToken.None)
            .Returns(existingGroupAccounts);

        var query = new GetGroupAccountsQuery
        {
            IsActive = isActive
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.IsActive).Distinct().Should().ContainSingle().Which.Should().Be(isActive);
    }

    [Fact]
    public async Task GetGroupAccounts_WhenJournalEntryTypeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var existingGroupAccounts = new List<GroupAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = journalEntryType
            }
        };

        unitOfWork.GroupAccountRepository
            .GetAllAsync(Arg.Any<GroupAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<GroupAccountFilter>();
                return existingGroupAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetGroupAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);
    }

    [Fact]
    public async Task GetGroupAccounts_WhenNoRecordsFound_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        unitOfWork.GroupAccountRepository
            .GetAllAsync(Arg.Any<GroupAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetGroupAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(new KeyValuePair<string, string>(nameof(GetGroupAccountsQueryResult), "Group account not found."));
    }

    [Fact]
    public async Task GetGroupAccounts_WhenMultipleFiltersAreApplied_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;
        var accountTitle = faker.Finance.AccountName();

        var existingGroupAccounts = new List<GroupAccountEntity>
    {
        new()
        {
            Id = Guid.NewGuid(),
            AccountTitle = accountTitle,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Debit
        },
        new()
        {
            Id = Guid.NewGuid(),
            AccountTitle = accountTitle,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = journalEntryType
        }
    };

        unitOfWork.GroupAccountRepository
            .GetAllAsync(Arg.Any<GroupAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<GroupAccountFilter>();
                return existingGroupAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .Where(x => filter.Title == null || x.AccountTitle == filter.Title)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetGroupAccountsQuery
        {
            JournalEntryType = journalEntryType,
            Title = accountTitle
        };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(accountTitle);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);
    }

    [Fact]
    public async Task GetGroupAccounts_WhenRecordsExist_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var entity = new GroupAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            AccountDescription = faker.Lorem.Sentence(),
            JournalEntryType = JournalEntryType.Debit
        };

        unitOfWork.GroupAccountRepository
            .GetAllAsync(Arg.Any<GroupAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(new List<GroupAccountEntity> { entity }.AsReadOnly());

        var query = new GetGroupAccountsQuery { Title = entity.AccountTitle };
        var handler = new GetGroupAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);

        var mapped = result.Result.First();
        mapped.AccountTitle.Should().Be(entity.AccountTitle);
        mapped.AccountCode.Should().Be(entity.AccountCode);
        mapped.IsActive.Should().Be(entity.IsActive);
        mapped.AccountDescription.Should().Be(entity.AccountDescription);
        mapped.JournalEntryType.Should().Be(entity.JournalEntryType);
    }
}