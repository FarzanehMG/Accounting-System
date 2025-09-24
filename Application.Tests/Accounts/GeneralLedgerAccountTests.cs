using Application.Common;
using Application.Features.Accounts.Commands.GeneralLedgerAccount;
using Application.Features.Accounts.Queries.GeneralLedgerAccount;
using Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities.Accounts;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Accounts;

public class GeneralLedgerAccountTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateGeneralLedgerAccountHandler_WhenGeneralLedgerAccountCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createGeneralAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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
            AccountTitle = title,
            AccountCode = faker.Finance.AccountName(),
            IsActive = false,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        unitOfWork.GeneralLedgerAccountRepository
            .FindAccountByTitleAndGroupAndTypeAsync(title, groupAccountId, journalEntryType, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { existingGeneralLedgerAccount }));

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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
        result.ErrorMessages.Should().ContainKey("Code")
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

        var createGeneralLedgerAccountCommand = new CreateGeneralLedgerAccountCommand
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

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = Guid.Empty
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.GeneralLedgerAccountId))
            .WhoseValue.Should().Contain("'General Ledger Account Id' must not be empty.");
    }

    [Fact]
    public async Task UpdateGeneralLedgerAccountHandler_ShouldFail_WhenGeneralAccountNotExist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var existingGeneralLedgerAccountEntity = new GeneralLedgerAccountEntity
        {
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

        // Act
        var result = await updateGeneralLedgerAccountHandler.Handle(updateGeneralLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(updateGeneralLedgerAccountCommand.GeneralLedgerAccountId))
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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            JournalEntryType = new Patch<JournalEntryType>(JournalEntryType.Debit)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            GroupAccountId = new Patch<Guid>(Guid.NewGuid())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            GroupAccountId = new Patch<Guid>(Guid.Empty)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            IsActive = new Patch<bool>(false)
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = groupAccountId,
            IsActive = true
        };

        unitOfWork.GeneralLedgerAccountRepository.FindActiveAccountByTitleAndGroupAsync(title, groupAccountId, CancellationToken.None)
            .Returns(Task.FromResult<IReadOnlyList<GeneralLedgerAccountEntity>>(new List<GeneralLedgerAccountEntity> { duplicateGeneralLedgerAccountEntity }));

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            Title = title
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = code,
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLegerAccountId, CancellationToken.None).Returns(existingGeneralLedgerAccountEntity);

        //unitOfWork.HasChanges(existingGeneralLedgerAccountEntity).Returns(true);

        var updateGeneralLedgerAccountCommand = new UpdateGeneralLedgerAccountCommand
        {
            GeneralLedgerAccountId = generalLegerAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var updateGeneralLedgerAccountHandler = new UpdateGeneralLedgerAccountCommandHandler(unitOfWork);

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
    public async Task GetGeneralLedgerAccount_WithValidId_ShouldReturnAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetGeneralAccountByIdQueryHandler(unitOfWork);
        var faker = new Faker();
        var generalLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();

        var existingGeneralLedgerAccount = new GeneralLedgerAccountEntity
        {
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Debit,
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };

        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        // Act
        var request = new GetGeneralLedgerAccountByIdQuery
        {
            Id = generalLedgerAccountId
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result.AccountTitle.Should().Be(title);
    }

    [Fact]
    public async Task GetGeneralLedgerAccount_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetGeneralAccountByIdQueryHandler(unitOfWork);

        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((GeneralLedgerAccountEntity?)null);

        // Act
        var request = new GetGeneralLedgerAccountByIdQuery
        {
            Id = Guid.NewGuid()
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(request.Id))
            .WhoseValue.Should().Contain("General account not found.");
    }

    [Fact]
    public async Task GetGeneralLedgerAccount_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetGeneralAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetGeneralLedgerAccountByIdQuery
        {
            Id = Guid.Empty
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetGeneralLedgerAccountByIdQuery.Id) && x.Value.Contains("must not be empty"));

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenTitleIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        var existingGeneralLedgerAccounts = new List<GeneralLedgerAccountEntity>
        { 
            new()
            {
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GroupAccountId = Guid.NewGuid()
            },
            new()
            {
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GroupAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(new GeneralLedgerAccountFilter
            {
                Title = title
            }, CancellationToken.None)
            .Returns(existingGeneralLedgerAccounts);

        var query = new GetGeneralLedgerAccountsQuery
        {
            Title = title
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(title);

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenCodeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var code = faker.Finance.AccountName();

        var existingGeneralLedgerAccounts = new List<GeneralLedgerAccountEntity>
        {
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GroupAccountId = Guid.NewGuid()
            },
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GroupAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(new GeneralLedgerAccountFilter
            {
                Code = code
            }, CancellationToken.None)
            .Returns(existingGeneralLedgerAccounts);

        var query = new GetGeneralLedgerAccountsQuery
        {
            Code = code
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountCode).Distinct().Should().ContainSingle().Which.Should().Be(code);

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenIsActiveIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const bool isActive = true;

        var existingGeneralLedgerAccounts = new List<GeneralLedgerAccountEntity>
        {
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GroupAccountId = Guid.NewGuid()
            },
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GroupAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(new GeneralLedgerAccountFilter
            {
                IsActive = true
            }, CancellationToken.None)
            .Returns(existingGeneralLedgerAccounts);

        var query = new GetGeneralLedgerAccountsQuery
        {
            IsActive = isActive
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.IsActive).Distinct().Should().ContainSingle().Which.Should().Be(isActive);

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenJournalEntryTypeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var existingGeneralLedgerAccounts = new List<GeneralLedgerAccountEntity>
        {
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GroupAccountId = Guid.NewGuid()
            },
            new()
            {
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = journalEntryType,
                GroupAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(Arg.Any<GeneralLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<GeneralLedgerAccountFilter>();
                return existingGeneralLedgerAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetGeneralLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenNoRecordsFound_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(Arg.Any<GeneralLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetGeneralLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(new KeyValuePair<string, string>(nameof(GetGeneralLedgerAccountsQueryResult), "General account not found."));

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenMultipleFiltersAreApplied_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;
        var accountTitle = faker.Finance.AccountName();

        var existingGeneralLedgerAccounts = new List<GeneralLedgerAccountEntity>
        {
            new()
            {
                AccountTitle = accountTitle,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GroupAccountId = Guid.NewGuid()
            },
            new()
            {
                AccountTitle = accountTitle,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = journalEntryType,
                GroupAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(Arg.Any<GeneralLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<GeneralLedgerAccountFilter>();
                return existingGeneralLedgerAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .Where(x => filter.Title == null || x.AccountTitle == filter.Title)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetGeneralLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType,
            Title = accountTitle
        };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(accountTitle);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);

    }

    [Fact]
    public async Task GetGeneralLedgerAccounts_WhenRecordsExist_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var entity = new GeneralLedgerAccountEntity
        {
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            AccountDescription = faker.Lorem.Sentence(),
            JournalEntryType = JournalEntryType.Debit,
            GroupAccountId = Guid.NewGuid()
        };

        unitOfWork.GeneralLedgerAccountRepository
            .GetAllAsync(Arg.Any<GeneralLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(new List<GeneralLedgerAccountEntity> { entity }.AsReadOnly());

        var query = new GetGeneralLedgerAccountsQuery { Title = entity.AccountTitle };
        var handler = new GetGeneralLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);

        var mapped = result.Result.First();
        mapped.AccountTitle.Should().Be(entity.AccountTitle);
        mapped.AccountCode.Should().Be(entity.AccountCode);
        mapped.IsActive.Should().Be(entity.IsActive);
        mapped.JournalEntryType.Should().Be(entity.JournalEntryType);
    }
}