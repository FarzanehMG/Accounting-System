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
            .WhoseValue.Should().Contain($"Account code already exists.");

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
}