using Application.Common;
using Application.Features.Accounts.Commands.SubLedgerAccount;
using Application.Features.Accounts.Queries.SubLedgerAccount;
using Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities.Accounts;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Accounts;

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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        // Act
        var result = await handler.Handle(subLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.SubLedgerAccountRepository.Received(1)
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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
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
        unitOfWork.SubLedgerAccountRepository
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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
        };
        unitOfWork.GeneralLedgerAccountRepository.GetByIdAsync(generalLedgerAccountId, CancellationToken.None)
            .Returns(existingGeneralLedgerAccount);

        unitOfWork.SubLedgerAccountRepository
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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
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
        unitOfWork.SubLedgerAccountRepository
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
            GroupAccountId = Guid.NewGuid(),
            IsActive = true
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
        unitOfWork.SubLedgerAccountRepository
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

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldFail_WhenSubLedgerAccountIdEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = Guid.Empty
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldFail_WhenSubLedgerAccountNotExist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = Guid.NewGuid()
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None)
            .Returns(null as SubLedgerAccountEntity);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(UpdateSubLegerAccountCommand))
            .WhoseValue.Should().Contain("SubLeger account not found.");
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldSuccess_WhenTitleNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingSubLedgerAccount.AccountTitle.Should().Be(updateSubLedgerAccountCommand.Title);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldSuccess_WhenIsActiveNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            IsActive = new Patch<bool>(false)
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingSubLedgerAccount.IsActive.Should().Be(updateSubLedgerAccountCommand.IsActive);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldSuccess_WhenCodeNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Code = code
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        unitOfWork.SubLedgerAccountRepository.FindByCodeAsync(code, CancellationToken.None)
            .Returns(null as SubLedgerAccountEntity);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingSubLedgerAccount.AccountCode.Should().Be(updateSubLedgerAccountCommand.Code);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldFail_WhenDuplicateActiveTitleExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Title = new Patch<string>(title)
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Title")
            .WhoseValue.Should().Contain("Active subLeger account exist.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldFail_WhenAccountCodeAlreadyExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var code = faker.Finance.Account();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Code = new Patch<string>(code)
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.SubLedgerAccountRepository.FindByCodeAsync(code, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Code")
            .WhoseValue.Should().Contain("Account code already exists. Please provide a unique code.");

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingSubLedgerAccount.AccountTitle.Should().Be(updateSubLedgerAccountCommand.Title);
        existingSubLedgerAccount.AccountCode.Should().Be(updateSubLedgerAccountCommand.Code);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldUpdateModifiedDate_WhenAccountIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var updateSubLedgerAccountCommand = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
            Code = new Patch<string>(faker.Finance.Account()),
            Title = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            IsActive = true,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            ModifiedDate = oldModifiedDate
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        unitOfWork.HasChanges(existingSubLedgerAccount).Returns(true);

        // Act
        var result = await handler.Handle(updateSubLedgerAccountCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingSubLedgerAccount.AccountTitle.Should().Be(updateSubLedgerAccountCommand.Title);
        existingSubLedgerAccount.AccountCode.Should().Be(updateSubLedgerAccountCommand.Code);
        existingSubLedgerAccount.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateSubLedgerAccountHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();
        var code = faker.Finance.Account();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true,
            AccountCode = code,
            ModifiedDate = oldModifiedDate
        };
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingAccount);

        unitOfWork.HasChanges(existingAccount).Returns(false);

        var handler = new UpdateSubLegerAccountCommandHandler(unitOfWork);

        var command = new UpdateSubLegerAccountCommand
        {
            SubLegerAccountId = subLedgerAccountId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingAccount.ModifiedDate.Should().Be(oldModifiedDate);

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GetSubLedgerAccount_WithValidId_ShouldReturnAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetSubLedgerAccountByIdQueryHandler(unitOfWork);
        var faker = new Faker();
        var subLedgerAccountId = Guid.NewGuid();
        var title = faker.Finance.AccountName();

        var existingSubLedgerAccount = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = title,
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Debit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccount);

        // Act
        var request = new GetSubLedgerAccountByIdQuery
        {
            Id = subLedgerAccountId
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Id.Should().Be(subLedgerAccountId);
        result.Result.AccountTitle.Should().Be(title);
    }

    [Fact]
    public async Task GetSubLedgerAccount_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetSubLedgerAccountByIdQueryHandler(unitOfWork);

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((SubLedgerAccountEntity?)null);

        // Act
        var request = new GetSubLedgerAccountByIdQuery
        {
            Id = Guid.NewGuid()
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(request.Id))
            .WhoseValue.Should().Contain("SubLedger account not found");
    }

    [Fact]
    public async Task GetSubLedgerAccount_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetSubLedgerAccountByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetSubLedgerAccountByIdQuery
        {
            Id = Guid.Empty
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetSubLedgerAccountByIdQuery.Id) && x.Value.Contains("must not be empty"));

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenTitleIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var title = faker.Finance.AccountName();

        var existingSubLedgerAccounts = new List<SubLedgerAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GeneralLedgerAccountId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = title,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GeneralLedgerAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(new SubLedgerAccountFilter
            {
                Title = title
            }, CancellationToken.None)
            .Returns(existingSubLedgerAccounts);

        var query = new GetSubLedgerAccountsQuery
        {
            Title = title
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(title);

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenCodeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var code = faker.Finance.AccountName();

        var existingSubLedgerAccounts = new List<SubLedgerAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GeneralLedgerAccountId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = code,
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GeneralLedgerAccountId  = Guid.NewGuid()
            }
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(new SubLedgerAccountFilter
            {
                Code = code
            }, CancellationToken.None)
            .Returns(existingSubLedgerAccounts);

        var query = new GetSubLedgerAccountsQuery
        {
            Code = code
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.AccountCode).Distinct().Should().ContainSingle().Which.Should().Be(code);

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenIsActiveIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const bool isActive = true;

        var existingSubLedgerAccounts = new List<SubLedgerAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GeneralLedgerAccountId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Credit,
                GeneralLedgerAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(new SubLedgerAccountFilter
            {
                IsActive = true
            }, CancellationToken.None)
            .Returns(existingSubLedgerAccounts);

        var query = new GetSubLedgerAccountsQuery
        {
            IsActive = isActive
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.IsActive).Distinct().Should().ContainSingle().Which.Should().Be(isActive);

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenJournalEntryTypeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        var existingSubLedgerAccounts = new List<SubLedgerAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GeneralLedgerAccountId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = faker.Finance.AccountName(),
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = journalEntryType,
                GeneralLedgerAccountId  = Guid.NewGuid()
            }
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(Arg.Any<SubLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<SubLedgerAccountFilter>();
                return existingSubLedgerAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetSubLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenNoRecordsFound_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(Arg.Any<SubLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetSubLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(new KeyValuePair<string, string>(nameof(GetSubLedgerAccountsQueryResult), "SubLedger account not found."));

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenMultipleFiltersAreApplied_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        const JournalEntryType journalEntryType = JournalEntryType.Credit;
        var accountTitle = faker.Finance.AccountName();

        var existingSubLedgerAccounts = new List<SubLedgerAccountEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = accountTitle,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = JournalEntryType.Debit,
                GeneralLedgerAccountId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountTitle = accountTitle,
                AccountCode = faker.Finance.AccountName(),
                IsActive = true,
                JournalEntryType = journalEntryType,
                GeneralLedgerAccountId = Guid.NewGuid()
            }
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(Arg.Any<SubLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<SubLedgerAccountFilter>();
                return existingSubLedgerAccounts
                    .Where(x => filter.JournalEntryType == null || x.JournalEntryType == filter.JournalEntryType)
                    .Where(x => filter.Title == null || x.AccountTitle == filter.Title)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetSubLedgerAccountsQuery
        {
            JournalEntryType = journalEntryType,
            Title = accountTitle
        };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.AccountTitle).Distinct().Should().ContainSingle().Which.Should().Be(accountTitle);
        result.Result.Select(x => x.JournalEntryType).Distinct().Should().ContainSingle().Which.Should().Be(journalEntryType);

    }

    [Fact]
    public async Task GetSubLedgerAccounts_WhenRecordsExist_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var entity = new SubLedgerAccountEntity
        {
            Id = Guid.NewGuid(),
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.AccountName(),
            IsActive = true,
            AccountDescription = faker.Lorem.Sentence(),
            JournalEntryType = JournalEntryType.Debit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.SubLedgerAccountRepository
            .GetAllAsync(Arg.Any<SubLedgerAccountFilter>(), Arg.Any<CancellationToken>())
            .Returns(new List<SubLedgerAccountEntity> { entity }.AsReadOnly());

        var query = new GetSubLedgerAccountsQuery { Title = entity.AccountTitle };
        var handler = new GetSubLedgerAccountsQueryHandler(unitOfWork);

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