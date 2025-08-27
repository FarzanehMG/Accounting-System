using Application.Common;
using Application.Features.Accounts.Commands.GeneralAccount;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests;

public class GeneralAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateGGeneralLedgerAccountHandler_WhenGeneralLedgerAccountCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGeneralAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            Description = null,
            GroupAccountId = Guid.NewGuid()
        };
        var createGeneralAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralAccountCommandHandler.Handle(createGeneralAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.GeneralLedgerAccountRepository.Received(1)
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = "",
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GroupAccountId = Guid.NewGuid()
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenJournalEntryTypeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = faker.Finance.Account(),
            JournalEntryType = 0,
            IsActive = true,
            GroupAccountId = Guid.NewGuid()
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenCodeIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GroupAccountId = Guid.NewGuid()
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

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

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = "",
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GroupAccountId = Guid.NewGuid()
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive().CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenExistSameActiveTitle()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();
        var groupAccountId = Guid.NewGuid();
        const JournalEntryType journalEntryType = JournalEntryType.Debit;

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            IsActive = true,
            GroupAccountId = groupAccountId
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(createGeneralLedgerAccountCommand.Title))
            .WhoseValue.Should().Contain("An active account with this title already exists in the selected group.");

        await unitOfWork.GeneralLedgerAccountRepository.DidNotReceive()
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralAccountHandler_ShouldSuccess_WhenExistSameActiveTitleWithDifferentJournalEntryType()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();
        var groupAccountId = Guid.NewGuid();
        const JournalEntryType journalEntryType = JournalEntryType.Debit;

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = true,
            GroupAccountId = groupAccountId
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.GeneralLedgerAccountRepository.Received(1)
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenInactiveAccountExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();
        var groupAccountId = Guid.NewGuid();
        const JournalEntryType journalEntryType = JournalEntryType.Debit;

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = false,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = title,
            Code = faker.Finance.Account(),
            JournalEntryType = journalEntryType,
            IsActive = true,
            GroupAccountId = groupAccountId
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(createGeneralLedgerAccountCommand.Title))
            .WhoseValue.Should().Contain($"An inactive account with this title already exists (Code: {existingGeneralLedgerAccount.AccountCode}). Consider reactivating it instead of creating a new one.");

        await unitOfWork.GeneralLedgerAccountRepository.DidNotReceive()
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenDuplicateAccountCodeExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();
        var groupAccountId = Guid.NewGuid();
        var accountCode = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Debit;

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = title,
            AccountCode = accountCode,
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        unitOfWork.GeneralLedgerAccountRepository
            .FindByCodeAsync(accountCode, Arg.Any<CancellationToken>())
            .Returns(existingGeneralLedgerAccount);

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = accountCode,
            JournalEntryType = journalEntryType,
            IsActive = true,
            GroupAccountId = groupAccountId
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(createGeneralLedgerAccountCommand.Code))
            .WhoseValue.Should().Contain("Account code already exists.");

        await unitOfWork.GeneralLedgerAccountRepository.DidNotReceive()
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_ShouldFail_WhenGroupAccountIdIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var accountCode = faker.Finance.AccountName();
        const JournalEntryType journalEntryType = JournalEntryType.Debit;

        var createGeneralLedgerAccountCommand = new CreateGeneralAccountCommand
        {
            Title = faker.Finance.AccountName(),
            Code = accountCode,
            JournalEntryType = journalEntryType,
            IsActive = true,
            GroupAccountId = Guid.Empty
        };
        var createGeneralLedgerAccountCommandHandler = new CreateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await createGeneralLedgerAccountCommandHandler.Handle(createGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(createGeneralLedgerAccountCommand.GroupAccountId))
            .WhoseValue.Should().Contain("'Group Account Id' must not be empty.");

        await unitOfWork.GeneralLedgerAccountRepository.DidNotReceive()
            .CreateAsync(Arg.Any<GeneralLedgerAccountEntity>(), CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenGeneralAccountIdEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = Guid.Empty
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.GeneralAccountId))
            .WhoseValue.Should().Contain("'General Account Id' must not be empty.");
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenGeneralAccountNotExist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = Guid.NewGuid()
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.GeneralAccountId))
            .WhoseValue.Should().Contain("General Account Not Found.");
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSuccess_WhenTitleNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.AccountTitle.Should().Be(updateGeneralLedgerAccountCommand.Title!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSuccess_WhenJournalEntryTypeNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            JournalEntryType = new Patch<JournalEntryType>(JournalEntryType.Debit)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.JournalEntryType.Should().Be(updateGeneralLedgerAccountCommand.JournalEntryType!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSuccess_WhenGroupAccountIdNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            GroupAccountId = new Patch<Guid>(Guid.NewGuid())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.GroupAccountId.Should().Be(updateGeneralLedgerAccountCommand.GroupAccountId!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenGroupAccountIdIsEmptyGuid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            GroupAccountId = new Patch<Guid>(Guid.Empty)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.GroupAccountId))
            .WhoseValue.Should().Contain("GroupAccountId cannot be empty.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSuccess_WhenIsActiveNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            IsActive = new Patch<bool>(false)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.IsActive.Should().Be(updateGeneralLedgerAccountCommand.IsActive!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSuccess_WhenCodeNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.AccountCode.Should().Be(updateGeneralLedgerAccountCommand.Code!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenDuplicateActiveTitleExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();
        var groupAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccountId,
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.GeneralLedgerAccountRepository.FindActiveAccountByTitleAndGroupAsync(title, groupAccountId, CancellationToken.None)
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccountEntity }));

        var duplicateGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = Guid.NewGuid(), // different from generalLegerAccountId
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccountId,
            IsActive = true
        };

        unitOfWork.GeneralLedgerAccountRepository.FindActiveAccountByTitleAndGroupAsync(title, groupAccountId, CancellationToken.None)
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { duplicateGeneralLedgerAccountEntity }));

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            Title = title
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.Title))
            .WhoseValue.Should().Contain("An active account with this title already exists in the selected group. Consider renaming or reactivating the existing account.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenAccountCodeAlreadyExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = code,
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        unitOfWork.GeneralLedgerAccountRepository.FindByCodeAsync(updateGeneralLedgerAccountCommand.Code, CancellationToken.None)
            .Returns(existingGeneralLedgerAccountEntity);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.Code))
            .WhoseValue.Should().Contain("Account code already exists. Please provide a unique code.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLegerAccountId = Guid.NewGuid();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLegerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.AccountCode.Should().Be(updateGeneralLedgerAccountCommand.Code!.Value);
        existingGeneralLedgerAccountEntity.AccountTitle.Should().Be(updateGeneralLedgerAccountCommand.Title!.Value);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldUpdateModifiedDate_WhenAccountIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccountEntity);

        unitOfWork.GeneralLedgerAccountRepository
            .FindActiveAccountByTitleAndGroupAsync(title, generalLedgerAccountId, CancellationToken.None)
            .Returns(new List<GeneralLedgerAccountEntity>());

        unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLedgerAccountId,
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingGeneralLedgerAccountEntity.ModifiedDate.Should().BeAfter(oldModifiedDate.Date);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        var code = faker.Finance.Account();
        var groupAccountId = Guid.NewGuid();

        var existingAccount = new GeneralLedgerAccountEntity
        {
            Id = generalLedgerAccountId,
            AccountTitle = title,
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccountId,
            IsActive = true,
            AccountCode = code,
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingAccount);

        unitOfWork.HasChanges(existingAccount).Returns(false);

        var handler = new UpdateGeneralAccountCommandHandler(unitOfWork);

        var command = new UpdateGeneralAccountCommand
        {
            GeneralAccountId = generalLedgerAccountId,
            Title = new Patch<string>(title),
            JournalEntryType = new Patch<JournalEntryType>(JournalEntryType.Credit),
            GroupAccountId = new Patch<Guid>(groupAccountId),
            IsActive = new Patch<bool>(true),
            Code = new Patch<string>(code)
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