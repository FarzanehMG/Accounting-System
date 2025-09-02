using Application.Features.Accounts.Commands.SubLedgerAccount;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests;

public class SubLedgerAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateSubLedgerAccountHandler_WhenSubLedgerAccountCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.SubLegerAccountRepository.Received(1)
            .CreateAsync(Arg.Any<SubLedgerAccountEntity>(), CancellationToken.None);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = "",
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenJournalEntryTypeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = 0,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenCodeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenGeneralLedgerAccountIdIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = Guid.Empty
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenSubLedgerAccountNatureNotSameAsGeneralLedgerAccount()
    {

        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(subLedgerAccountCommand.JournalEntryType))
            .WhoseValue.Should().Contain("Sub Ledger account must have the same nature (JournalEntryType) as its General Ledger account.");
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = 0,
            IsActive = true,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive().CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenExistSameActiveTitle()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        unitOfWork.SubLegerAccountRepository
            .FindAccountByTitleAndGeneralLedgerAndTypeAsync(title, generalLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<SubLedgerAccountEntity> { existingSubLedgerAccount });

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain("An active account with this title already exists in the selected generalLedger.");
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldSuccess_WhenExistSameActiveTitleWithDifferentJournalEntryType()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        unitOfWork.SubLegerAccountRepository
            .FindAccountByTitleAndGeneralLedgerAndTypeAsync(title, generalLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<SubLedgerAccountEntity>());

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenInactiveAccountExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;
        var code = faker.Finance.Account();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = title,
            Code = code,
            Description = null,
            JournalEntryType = journalEntryType,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = code,
            IsActive = false,
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        unitOfWork.SubLegerAccountRepository
            .FindAccountByTitleAndGeneralLedgerAndTypeAsync(title, generalLedgerAccountId, journalEntryType, CancellationToken.None)
            .Returns(new List<SubLedgerAccountEntity> { existingSubLedgerAccount });

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain($"An inactive account with this title already exists (Code: {code}). Consider reactivating it instead of creating a new one.");
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenDuplicateAccountCodeExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = code,
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid()
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = code,
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        unitOfWork.SubLegerAccountRepository
            .FindByCodeAsync(code, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Code")
            .WhoseValue.Should().Contain("Account code already exists.");

    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenGeneralLedgerNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("GeneralLedgerAccountId")
            .WhoseValue.Should().Contain("General ledger not found.");
    }

    [Fact]
    public async Task CreateSubLedgerAccountHandler_ShouldFail_WhenGeneralLedgerIsInactive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();

        var subLedgerAccountCommand = new CreateSubLedgerAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            Description = null,
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        var handler = new CreateSubLedgerAccountCommandHandler(unitOfWork);

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = false
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId)
            .Returns(existingGeneralLedgerAccount);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("GeneralLedgerAccountId")
            .WhoseValue.Should().Contain("The General Ledger account must be active.");
    }
}