using Application.Features.Journal.Commands.JournalEntry;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Journal;

public class JournalEntryTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Credit && e.Amount == 2), CancellationToken.None);

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Debit && e.Amount == 2), CancellationToken.None);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenCreateJournalEntryCommandLessThanTwoRecords()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Entries")
            .WhoseValue.Should().Contain("At least two journal entry is required.");
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenCreateJournalEntryCommandLessNoRecord()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>();

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Entries")
            .WhoseValue.Should().Contain("At least two journal entry is required.");
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAllCommandsNotTargetTheSameJournalVoucherId()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = Guid.NewGuid()
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucherId")
            .WhoseValue.Should().Contain("All entries must belong to the same journal voucher.");
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDebitAndCreditNotBalanced()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 5,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Entries")
            .WhoseValue.Should().Contain("Journal entries are not balanced. Debit and Credit totals must match.");
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_CommandsNotValid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 0,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 0,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
                .Returns((JournalVoucherEntity?)null);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucherId")
            .WhoseValue.Should().Contain("The journal voucher was not found.");

        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Approved,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucherState")
            .WhoseValue.Should().Contain("Cannot add entries to a non-Draft voucher.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns((SubLedgerAccountEntity?)null);

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("This subLedger account was not found.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker, false));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("This subLedger account is not active.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithSubLedgerAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Debit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalEntryType")
            .WhoseValue.Should().Contain("Journal entry type does not match subLedger account.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns((DetailedAccountEntity?) null);


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("DetailedAccountId")
            .WhoseValue.Should().Contain("This detailed account was not found.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker, false));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("DetailedAccountId")
            .WhoseValue.Should().Contain("This detailed account is not active.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithDetailedAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Credit, faker));


        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalEntryType")
            .WhoseValue.Should().Contain("Journal entry type does not match detailed account.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAssignIncrementalNumber_PerJournalVoucher()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId, subLedgerAccountId2, JournalEntryType.Debit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                DetailedAccountId = detailedAccountId,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Credit && e.Amount == 2 && e.JournalEntryNumber == "1"), CancellationToken.None);

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Debit && e.Amount == 2 && e.JournalEntryNumber == "2"), CancellationToken.None);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAllowNullDetailedAccountId()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Credit && e.Amount == 2 && e.JournalEntryNumber == "1"), CancellationToken.None);

        await unitOfWork.JournalEntryRepository.Received(1)
            .CreateAsync(Arg.Is<JournalEntryEntity>(e => e.JournalEntryType == JournalEntryType.Debit && e.Amount == 2 && e.JournalEntryNumber == "2"), CancellationToken.None);

    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 0,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldNotCommit_WhenOneEntryFailsValidation()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = Guid.Empty,
                JournalVoucherId = journalVoucherId
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = subLedgerAccountId2,
                JournalVoucherId = journalVoucherId
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);

        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var createJournalEntryCommand = new List<CreateJournalEntryCommand>
        {
            new()
            {
                JournalEntryType = JournalEntryType.Credit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = Guid.Empty,
                JournalVoucherId = Guid.NewGuid()
            },
            new()
            {
                JournalEntryType = JournalEntryType.Debit,
                Amount = 2,
                Description = null,
                SubLedgerAccountId = Guid.NewGuid(),
                JournalVoucherId = Guid.NewGuid()
            }
        };

        var handle = new CreateJournalEntryCommandHandler(unitOfWork);

        // Act
        await handle.Handle(createJournalEntryCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenValidationIsFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenParentJournalVoucherNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenFinancialPeriodInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task WhenAmountIsZeroOrNegative_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchSubLedgerAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task WhenAmountIsZeroOrNegative_ShouldFail_WhenDetailedAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchDetailedAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenAmountNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDescriptionNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenSubLedgerAccountIdNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDetailedAccountIdNotEmpty()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldUpdateModifiedDate_WhenJournalEntryIsUpdated()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldNotCommit_WhenNoFieldsChanged()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenValidationFails()
    {
        throw new NotImplementedException();
    }
}