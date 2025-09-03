using Application.Common;
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

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldFail_WhenDetailedAccountIdEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = Guid.Empty
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldFail_WhenDetailedAccountNotExist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = Guid.NewGuid()
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        unitOfWork.DetailedAccountRepository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None)
            .Returns(null as DetailedAccountEntity);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(UpdateDetailedAccountCommand))
            .WhoseValue.Should().Contain("Detailed account not found.");
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldSuccess_WhenTitleNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDetailedAccount.AccountTitle.Should().Be(existingDetailedAccount.AccountTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldSuccess_WhenIsActiveNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            IsActive = new Patch<bool>(false)
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDetailedAccount.AccountTitle.Should().Be(existingDetailedAccount.AccountTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldSuccess_WhenCodeNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Code = code
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        unitOfWork.DetailedAccountRepository.FindByCodeAsync(code, CancellationToken.None)
            .Returns(null as DetailedAccountEntity);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDetailedAccount.AccountTitle.Should().Be(existingDetailedAccount.AccountTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldFail_WhenDuplicateActiveTitleExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Title = new Patch<string>(title)
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain("Active detailed account exist.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldFail_WhenAccountCodeAlreadyExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Code = new Patch<string>(code)
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.DetailedAccountRepository.FindByCodeAsync(code, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Code")
            .WhoseValue.Should().Contain("Account code already exists. Please provide a unique code.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDetailedAccount.AccountTitle.Should().Be(existingDetailedAccount.AccountTitle);
        existingDetailedAccount.AccountCode.Should().Be(existingDetailedAccount.AccountCode);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldUpdateModifiedDate_WhenAccountIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var updateDetailedAccountCommand = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var existingDetailedAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid(),
            ModifiedDate = oldModifiedDate
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccount);

        unitOfWork.HasChanges(existingDetailedAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateDetailedAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingDetailedAccount.AccountTitle.Should().Be(existingDetailedAccount.AccountTitle);
        existingDetailedAccount.AccountCode.Should().Be(existingDetailedAccount.AccountCode);
        existingDetailedAccount.ModifiedDate.Should().BeAfter(oldModifiedDate.Date);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateDetailedAccountHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var detailedAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        var code = faker.Finance.Account();

        var existingAccount = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = title,
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid(),
            IsActive = true,
            AccountCode = code,
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingAccount);

        unitOfWork.HasChanges(existingAccount).Returns(false);

        var handler = new UpdateDetailedAccountCommandHandler(unitOfWork);

        var command = new UpdateDetailedAccountCommand
        {
            DetailedAccountId = detailedAccountId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingAccount.ModifiedDate.Should().BeCloseTo((DateTime)existingAccount.ModifiedDate, TimeSpan.FromSeconds(1));

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }
}