using Application.Features.Accounts.Commands.DetailedAccount;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests;

public class DetailedAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateDetailedAccountHandler_WhenDetailedAccountCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.DetailedAccountRepository.Received(1)
            .CreateAsync(Arg.Any<DetailedAccountEntity>(), CancellationToken.None);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = "",
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenJournalEntryTypeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = 0,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenCodeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenSubLedgerAccountIdIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = Guid.Empty
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenDetailedAccountNatureNotSameAsSubLedgerAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId,
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(detailedAccountCommand.JournalEntryType))
            .WhoseValue.Should().Contain("Detailed account must have the same nature (JournalEntryType) as its SubLedger account.");
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = 0,
            IsActive = true,
            SubLedgerAccountId = Guid.NewGuid()
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive().CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenExistSameActiveTitle()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            SubLedgerAccountId = subLedgerAccountId
        };
        unitOfWork.DetailedAccountRepository
            .FindAccountByTitleAndSubLedgerAndTypeAsync(title, subLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<DetailedAccountEntity> { existingDetailedAccount });

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain("An active account with this title already exists in the selected subLedger.");
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldSuccess_WhenExistSameActiveTitleWithDifferentJournalEntryType()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.DetailedAccountRepository
            .FindAccountByTitleAndSubLedgerAndTypeAsync(title, subLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<DetailedAccountEntity>());

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenInactiveAccountExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;
        var code = faker.Finance.Account();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = title,
            Code = code,
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = code,
            IsActive = false,
            JournalEntryType = journalEntryType,
            SubLedgerAccountId = subLedgerAccountId,
        };
        unitOfWork.DetailedAccountRepository
            .FindAccountByTitleAndSubLedgerAndTypeAsync(title, subLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<DetailedAccountEntity> { existingDetailedAccount });

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain($"An inactive account with this title already exists (Code: {code}). Consider reactivating it instead of creating a new one.");
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenDuplicateAccountCodeExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = code,
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = code,
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = subLedgerAccountId
        };
        unitOfWork.DetailedAccountRepository
            .FindByCodeAsync(code, CancellationToken.None)
            .Returns(existingDetailedAccount);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Code")
            .WhoseValue.Should().Contain("Account code already exists.");
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenSubLedgerNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = Guid.NewGuid()
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("SubLedger not found.");
    }

    [Fact]
    public async Task CreateDetailedAccountHandler_ShouldFail_WhenSubLedgerIsInactive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var detailedAccountCommand = new CreateDetailedAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            SubLedgerAccountId = subLedgerAccountId
        };
        var handler = new CreateDetailedAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = false
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId)
            .Returns(existingSubLedgerAccount);

        // Act
        var result = await handler.Handle(detailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("The Sub Ledger account must be active.");
    }
}