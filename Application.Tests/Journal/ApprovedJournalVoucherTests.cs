using Application.Features.Journal.Commands.JournalVoucher;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities.Accounts;
using Domain.Entities.Journal;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Journal;

public class ApprovedJournalVoucherTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldSucceed_WhenValidationIsValid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get SubLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.JournalVoucherState.Should().Be(JournalVoucherState.Approved);
        existingJournalVoucherEntity.ApprovedByUserId.Should().Be(approvedByUserId);
        existingJournalVoucherEntity.ApprovedTime.Should().NotBeNull();
        existingJournalVoucherEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenValidationIsNotValid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherId = Guid.NewGuid();

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = Guid.Empty,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Arrange
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenJournalVoucherNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherId = Guid.NewGuid();

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = Guid.NewGuid()
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns((JournalVoucherEntity?)null);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Arrange
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucherId")
            .WhoseValue.Should().Contain("This journal voucher was not found.");
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenJournalVoucherNotInDraftState()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Approved,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucherState")
            .WhoseValue.Should().Contain("Journal voucher must be in Draft state to approve.");
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenJournalEntriesLessThanTwo()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 2,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("Entries")
            .WhoseValue.Should().Contain("Cannot approve voucher without at least two journal entries.");
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenSubLedgerAccountInactive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get SubLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, JournalEntryType.Credit, faker, false));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("SubLedgerAccount")
            .WhoseValue.Should().Contain($"SubLedger account for entry {existingJournalVoucherEntity.JournalEntries.First().JournalEntryNumber} is not active.");
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenDetailedAccountInactive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId1 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    DetailedAccountId = detailedAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get subLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Get detailed account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId1, CancellationToken.None)
            .Returns(Helper.CreateDetailedAccount(detailedAccountId1,subLedgerAccountId1, JournalEntryType.Credit, faker, false));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("DetailedAccount")
            .WhoseValue.Should().Contain($"Detailed account for entry {existingJournalVoucherEntity.JournalEntries.First().JournalEntryNumber} is not active.");

    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenJournalEntryTypeInvalid()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = 0,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get subLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, 0, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("JournalEntryType")
            .WhoseValue.Should().Contain("Invalid journal entry type.");

    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenJournalEntriesNotBalanced()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 2,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get subLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("JournalEntryBalance")
            .WhoseValue.Should().Contain("Journal entries are not balanced.");
    }

    // ===============================================================================================================

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get SubLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns((SubLedgerAccountEntity?)null);
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("SubLedgerAccount")
            .WhoseValue.Should().Contain("SubLedger account not found.");

    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldFail_WhenDetailedAccountNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var detailedAccountId = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId1,
                    DetailedAccountId = detailedAccountId,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get SubLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Get detailed account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, CancellationToken.None)
            .Returns((DetailedAccountEntity?)null);

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);

        result.ErrorMessages.Should().ContainKey("DetailedAccount")
            .WhoseValue.Should().Contain("Detailed account not found.");
    }

    [Fact]
    public async Task ApprovedJournalVoucherHandler_ShouldSucceed_WhenMultipleBalancedEntriesExist()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var subLedgerAccountId1 = Guid.NewGuid();
        var subLedgerAccountId2 = Guid.NewGuid();
        var subLedgerAccountId3 = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        // Get journal voucher
        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ApprovedByUserId = null,
            ApprovedTime = null,
            ModifiedDate = oldModifiedDate,
            JournalEntries = new List<JournalEntryEntity>
            {
                new()
                {
                    JournalEntryNumber = "1",
                    JournalEntryType = JournalEntryType.Credit,
                    Amount = 10,
                    SubLedgerAccountId = subLedgerAccountId1,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "2",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId2,
                    JournalVoucherId = journalVoucherId
                },
                new()
                {
                    JournalEntryNumber = "3",
                    JournalEntryType = JournalEntryType.Debit,
                    Amount = 5,
                    SubLedgerAccountId = subLedgerAccountId3,
                    JournalVoucherId = journalVoucherId
                }
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdWithEntriesAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Get SubLedger accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId1, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId1, JournalEntryType.Credit, faker));

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId3, CancellationToken.None)
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId3, JournalEntryType.Debit, faker));

        var approvedJournalVoucherCommand = new ApprovedJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ApprovedByUserId = approvedByUserId,
        };

        var handler = new ApprovedJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(approvedJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.JournalVoucherState.Should().Be(JournalVoucherState.Approved);
        existingJournalVoucherEntity.ApprovedByUserId.Should().Be(approvedByUserId);
        existingJournalVoucherEntity.ApprovedTime.Should().NotBeNull();
        existingJournalVoucherEntity.ModifiedDate.Should().BeAfter(oldModifiedDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

}