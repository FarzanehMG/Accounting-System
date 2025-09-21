using Application.Common;
using Application.Features.Journal.Commands.JournalEntry;
using Application.Features.Journal.Queries.JournalEntry;
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
            .Returns((DetailedAccountEntity?)null);


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
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.Amount.Should().Be(updateJournalEntryCommand.Amount);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);
        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenValidationIsFalse()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = Guid.Empty,
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns((JournalEntryEntity?)null);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalEntryId")
            .WhoseValue.Should().Contain("Journal entry not found.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenParentJournalVoucherNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns((JournalVoucherEntity?)null);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucher")
            .WhoseValue.Should().Contain("Journal voucher not found.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Approved,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucher")
            .WhoseValue.Should().Contain("Cannot update entry of a non-Draft voucher.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenFinancialPeriodInActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = false
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriod")
            .WhoseValue.Should().Contain("Financial period is closed. Cannot update entry.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(0)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);


        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Amount")
            .WhoseValue.Should().Contain("Amount must be greater than zero.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns((SubLedgerAccountEntity?)null);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            SubLedgerAccountId = new Patch<Guid>(subLedgerAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("SubLedger account not found.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingSubLedgerAccountEntity = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = false
        };

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccountEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            SubLedgerAccountId = new Patch<Guid>(subLedgerAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("SubLedgerAccountId")
            .WhoseValue.Should().Contain("SubLedger account is inactive.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchSubLedgerAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingSubLedgerAccountEntity = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Debit,
            GeneralLedgerAccountId = Guid.NewGuid(),
            IsActive = true
        };

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccountEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            SubLedgerAccountId = new Patch<Guid>(subLedgerAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalEntryType")
            .WhoseValue.Should().Contain("Entry type does not match subLedger account.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns((DetailedAccountEntity?)null);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("DetailedAccountId")
            .WhoseValue.Should().Contain("Detailed account not found.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingDetailedAccountEntity = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            IsActive = false,
            SubLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccountEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("DetailedAccountId")
            .WhoseValue.Should().Contain("Detailed account is inactive.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchDetailedAccount()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingDetailedAccountEntity = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true,
            SubLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccountEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalEntryType")
            .WhoseValue.Should().Contain("Entry type does not match detailed account.");
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenAmountNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.Amount.Should().Be(updateJournalEntryCommand.Amount);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDescriptionNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            Description = new Patch<string>(faker.Finance.AccountName())
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.Description.Should().Be(updateJournalEntryCommand.Description);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenSubLedgerAccountIdNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingSubLedgerAccountEntity = new SubLedgerAccountEntity
        {
            Id = subLedgerAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            GeneralLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, CancellationToken.None)
            .Returns(existingSubLedgerAccountEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            SubLedgerAccountId = new Patch<Guid>(subLedgerAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.SubLedgerAccountId.Should().Be(updateJournalEntryCommand.SubLedgerAccountId);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSuccess_WhenDetailedAccountIdNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingDetailedAccountEntity = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccountEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.DetailedAccountId.Should().Be(updateJournalEntryCommand.DetailedAccountId);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingDetailedAccountEntity = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccountEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId),
            Amount = new Patch<decimal>(5)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.SubLedgerAccount.Should().Be(updateJournalEntryCommand.SubLedgerAccountId);
        existingJournalEntryEntity.Amount.Should().Be(updateJournalEntryCommand.Amount);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldUpdateModifiedDate_WhenJournalEntryIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId,
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var existingDetailedAccountEntity = new DetailedAccountEntity
        {
            Id = detailedAccountId,
            AccountTitle = faker.Finance.AccountName(),
            AccountCode = faker.Finance.Account(),
            JournalEntryType = JournalEntryType.Credit,
            SubLedgerAccountId = Guid.NewGuid()
        };

        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns(existingDetailedAccountEntity);

        unitOfWork.HasChanges(existingJournalEntryEntity)
            .Returns(true);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId,
            DetailedAccountId = new Patch<Guid>(detailedAccountId)
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalEntryEntity.DetailedAccountId.Should().Be(updateJournalEntryCommand.DetailedAccountId);
        existingJournalEntryEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateJournalEntryCommandHandler_ShouldNotCommit_WhenNoFieldsChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalEntryId = Guid.NewGuid();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = journalVoucherId
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            Id = journalVoucherId,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.Locale,
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalEntryCommand = new UpdateJournalEntryCommand
        {
            JournalEntryId = journalEntryId
        };

        var handler = new UpdateJournalEntryCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateJournalEntryCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task GetJournalEntry_WithValidId_ShouldReturnJournalEntry()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalEntryByIdQueryHandler(unitOfWork);
        var journalEntryId = Guid.NewGuid();

        var existingJournalEntryEntity = new JournalEntryEntity
        {
            Id = journalEntryId,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            JournalEntryNumber = "1",
            JournalEntryType = JournalEntryType.Credit,
            Amount = 2,
            SubLedgerAccountId = Guid.NewGuid(),
            JournalVoucherId = Guid.NewGuid()
        };

        unitOfWork.JournalEntryRepository.GetByIdAsync(journalEntryId, CancellationToken.None)
            .Returns(existingJournalEntryEntity);

        // Act
        var request = new GetJournalEntryByIdQuery
        {
            Id = journalEntryId
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Id.Should().Be(journalEntryId);
        result.Result.JournalEntryNumber.Should().Be(existingJournalEntryEntity.JournalEntryNumber);
    }

    [Fact]
    public async Task GetJournalEntry_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalEntryByIdQueryHandler(unitOfWork);

        unitOfWork.JournalEntryRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((JournalEntryEntity?)null);

        // Act
        var request = new GetJournalEntryByIdQuery
        {
            Id = Guid.NewGuid()
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(request.Id))
            .WhoseValue.Should().Contain("Journal entry not found");
    }

    [Fact]
    public async Task GetJournalEntry_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalEntryByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetJournalEntryByIdQuery
        {
            Id = Guid.Empty
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetJournalEntryByIdQuery.Id) && x.Value.Contains("must not be empty"));
    }
}