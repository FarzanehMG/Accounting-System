using Application.Features.Journal.Commands.JournalEntry;
using Application.Repositories.Common;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests.Journal;

public class JournalEntryTests
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
        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId)
            .Returns(existingJournalVoucherEntity);

        // SubLedger Accounts
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId, Arg.Any<CancellationToken>())
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId, JournalEntryType.Credit, faker));
        unitOfWork.SubLedgerAccountRepository.GetByIdAsync(subLedgerAccountId2, Arg.Any<CancellationToken>())
            .Returns(Helper.CreateSubLedgerAccount(subLedgerAccountId2, JournalEntryType.Debit, faker));

        // Detailed Account
        unitOfWork.DetailedAccountRepository.GetByIdAsync(detailedAccountId, Arg.Any<CancellationToken>())
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
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAllCommandsNotTargetTheSameJournalVoucherId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_CommandsNotValid()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalVoucherStateNotDraft()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenSubLedgerAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithSubLedgerAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountNotFound()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenDetailedAccountInActive()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenJournalEntryTypeNotMatchWithDetailedAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAssignIncrementalNumber_PerJournalVoucher()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAllowNullDetailedAccountId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldAllowNullDescription()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldNotCommit_WhenOneEntryFailsValidation()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateJournalEntryCommandHandler_ShouldThrowException_WhenRepositoryFails()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        throw new NotImplementedException();
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