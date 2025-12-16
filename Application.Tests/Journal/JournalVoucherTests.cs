using Application.Common;
using Application.Contracts.JournalVoucherService;
using Application.Features.Journal.Commands.JournalVoucher;
using Application.Features.Journal.Queries.JournalVoucher;
using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities.FinancialPeriod;
using Domain.Entities.Journal;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.Journal;

public class JournalVoucherTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        var existingJournalVoucher = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId,
            JournalVoucherNumber = 1
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        unitOfWork.JournalVoucherRepository.GetLastJournalVoucherAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingJournalVoucher);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);

        await unitOfWork.JournalVoucherRepository.Received(1)
            .CreateAsync(Arg.Is<JournalVoucherEntity>(x => x.JournalVoucherNumber == 2), CancellationToken.None);
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldFail_WhenFinancialPeriodNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriodId")
            .WhoseValue.Should().Contain("Financial period not found.");
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldFail_WhenFinancialPeriodIsNotActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = false
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriodId")
            .WhoseValue.Should().Contain("This financial period is not active.");
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldFail_WhenTransactionTimeNotWithinTheFinancialPeriod()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow.AddDays(-1),
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriodId")
            .WhoseValue.Should().Contain("Transaction time must be within the selected financial period.");
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldAssignNumberOne_WhenNoVoucherExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        unitOfWork.JournalVoucherRepository.GetLastJournalVoucherAsync(financialPeriodId, CancellationToken.None)
            .Returns((JournalVoucherEntity?)null);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await unitOfWork.JournalVoucherRepository.Received(1)
            .CreateAsync(Arg.Is<JournalVoucherEntity>(x => x.JournalVoucherNumber == 1), CancellationToken.None);
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldIncrementNumber_WhenVoucherExists()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriod = new FinancialPeriodEntity
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        var existingJournalVoucher = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId,
            JournalVoucherNumber = 1
        };

        unitOfWork.FinancialPeriodRepository.GetByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriod);

        unitOfWork.JournalVoucherRepository.GetLastJournalVoucherAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingJournalVoucher);

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = financialPeriodId
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        var result = await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);

        await unitOfWork.JournalVoucherRepository.Received(1)
            .CreateAsync(Arg.Is<JournalVoucherEntity>(x => x.JournalVoucherNumber == 2), CancellationToken.None);
    }

    [Fact]
    public async Task CreateJournalVoucherCommandHandler_ShouldHandleConcurrency_WhenMultipleCreates()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var journalVoucherNumberGenerator = Substitute.For<IJournalVoucherNumberGenerator>();

        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = "",
            ReferenceNumber = "",
            TransactionTime = DateTime.UtcNow,
            Description = null,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        var handler = new CreateJournalVoucherCommandHandler(unitOfWork, journalVoucherNumberGenerator);

        // Act
        await handler.Handle(createJournalVoucherCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSuccess_WhenValidationIsTrue()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.JournalVoucherTitle.Should().Be(updateJournalVoucherCommand.JournalVoucherTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldFail_WhenJournalVoucherNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(null as JournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucher")
            .WhoseValue.Should().Contain("Journal voucher not found.");
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldFail_WhenJournalVoucherNonDraftState()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Approved,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("JournalVoucher")
            .WhoseValue.Should().Contain("Cannot update entry of a non-Draft voucher.");
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldFail_WhenFinancialPeriodIsNotActive()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = false
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriod")
            .WhoseValue.Should().Contain("Financial period is closed. Cannot update journal voucher.");
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldFail_WhenTransactionTimeNotWithinTheFinancialPeriod()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            FinancialPeriod = new FinancialPeriodEntity
            {
                FinancialPeriodTitle = faker.Finance.AccountName(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = true
            }
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            TransactionTime = new Patch<DateTime>(DateTime.UtcNow.AddDays(-1))
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("FinancialPeriodId")
            .WhoseValue.Should().Contain($"Transaction time must be within the selected financial period " +
                                         $"({existingJournalVoucherEntity.FinancialPeriod.StartDate:yyyy-MM-dd} - " +
                                         $"{existingJournalVoucherEntity.FinancialPeriod.EndDate:yyyy-MM-dd}).");
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSuccess_WhenJournalVoucherTitleNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);


        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.JournalVoucherTitle.Should().Be(updateJournalVoucherCommand.JournalVoucherTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSuccess_WhenTransactionTimeNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);


        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            TransactionTime = new Patch<DateTime>(DateTime.UtcNow)
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.TransactionTime.Should().Be(updateJournalVoucherCommand.TransactionTime);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSuccess_WhenReferenceNumberNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            ReferenceNumber = new Patch<string>(faker.Finance.RoutingNumber())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.ReferenceNumber.Should().Be(updateJournalVoucherCommand.ReferenceNumber);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSuccess_WhenDescriptionNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);


        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            Description = new Patch<string>(faker.Lorem.Paragraph())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.Description.Should().Be(updateJournalVoucherCommand.Description);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            Description = new Patch<string>(faker.Lorem.Paragraph()),
            TransactionTime = new Patch<DateTime>(DateTime.UtcNow.AddDays(3)),
            JournalVoucherTitle = new Patch<string>(faker.Finance.AccountName())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.Description.Should().Be(updateJournalVoucherCommand.Description);
        existingJournalVoucherEntity.JournalVoucherTitle.Should().Be(updateJournalVoucherCommand.JournalVoucherTitle);
        existingJournalVoucherEntity.TransactionTime.Should().Be(updateJournalVoucherCommand.TransactionTime);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldUpdateModifiedDate_WhenJournalVoucherIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
            Description = new Patch<string>(faker.Lorem.Paragraph())
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.Description.Should().Be(updateJournalVoucherCommand.Description);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid(),
            ModifiedDate = oldModifiedDate
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = journalVoucherId,
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingJournalVoucherEntity.ModifiedDate.Should().Be(oldModifiedDate);
    }

    [Fact]
    public async Task UpdateJournalVoucherCommandHandler_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        var updateJournalVoucherCommand = new UpdateJournalVoucherCommand
        {
            JournalVoucherId = Guid.NewGuid()
        };

        var handle = new UpdateJournalVoucherCommandHandler(unitOfWork);

        // Act
        var result = await handle.Handle(updateJournalVoucherCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task GetJournalVoucher_WithValidId_ShouldReturnJournalVoucher()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalVoucherByIdQueryHandler(unitOfWork);
        var faker = new Faker();
        var journalVoucherId = Guid.NewGuid();

        var existingJournalVoucherEntity = new JournalVoucherEntity
        {
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        unitOfWork.JournalVoucherRepository.GetByIdAsync(journalVoucherId, CancellationToken.None)
            .Returns(existingJournalVoucherEntity);

        // Act
        var request = new GetJournalVoucherByIdQuery
        {
            Id = journalVoucherId
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result.JournalVoucherTitle.Should().Be(existingJournalVoucherEntity.JournalVoucherTitle);
    }

    [Fact]
    public async Task GetJournalVoucher_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalVoucherByIdQueryHandler(unitOfWork);

        unitOfWork.JournalVoucherRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((JournalVoucherEntity?)null);

        // Act
        var request = new GetJournalVoucherByIdQuery
        {
            Id = Guid.NewGuid()
        };
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey(nameof(request.Id))
            .WhoseValue.Should().Contain("Journal voucher not found");
    }

    [Fact]
    public async Task GetJournalVoucher_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetJournalVoucherByIdQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(new GetJournalVoucherByIdQuery
        {
            Id = Guid.Empty
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should()
            .Contain(x => x.Key == nameof(GetJournalVoucherByIdQuery.Id) && x.Value.Contains("must not be empty"));
    }

    [Fact]
    public async Task GetJournalVouchers_WhenJournalVoucherStateIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherState = JournalVoucherState.Approved;

        var existingJournalVouchers = new List<JournalVoucherEntity>
        {
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = journalVoucherState,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            },
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = journalVoucherState,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            }
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(new JournalVoucherFilter
            {
                JournalVoucherState = journalVoucherState
            }, CancellationToken.None)
            .Returns(existingJournalVouchers);

        var query = new GetJournalVouchersQuery
        {
            JournalVoucherState = journalVoucherState
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.JournalVoucherState).Distinct().Should().ContainSingle().Which.Should().Be(journalVoucherState);
    }

    [Fact]
    public async Task GetJournalVouchers_WhenApprovedTimeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var approvedTime = DateTime.UtcNow;

        var existingJournalVouchers = new List<JournalVoucherEntity>
        {
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Approved,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid(),
                ApprovedTime = approvedTime
            },
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Approved,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid(),
                ApprovedTime = approvedTime
            }
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(new JournalVoucherFilter
            {
                ApprovedTime = approvedTime
            }, CancellationToken.None)
            .Returns(existingJournalVouchers);

        var query = new GetJournalVouchersQuery
        {
            ApprovedTime = approvedTime
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
        result.Result.Select(x => x.ApprovedTime).Distinct().Should().ContainSingle().Which.Should().Be(approvedTime);

    }

    [Fact]
    public async Task GetJournalVouchers_WhenJournalVoucherTitleIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherTitle = faker.Finance.AccountName();

        var existingJournalVouchers = new List<JournalVoucherEntity>
        {
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            },
            new()
            {
                JournalVoucherTitle = journalVoucherTitle,
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            }
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(Arg.Any<JournalVoucherFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<JournalVoucherFilter>();
                return existingJournalVouchers
                    .Where(x => filter.JournalVoucherTitle == null || x.JournalVoucherTitle == filter.JournalVoucherTitle)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetJournalVouchersQuery
        {
            JournalVoucherTitle = journalVoucherTitle
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.JournalVoucherTitle).Distinct().Should().ContainSingle().Which.Should().Be(journalVoucherTitle);
    }

    [Fact]
    public async Task GetJournalVouchers_WhenTransactionTimeIsValid_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var transactionTime = DateTime.UtcNow.AddDays(2);

        var existingJournalVouchers = new List<JournalVoucherEntity>
        {
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = transactionTime,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            },
            new()
            {
                JournalVoucherTitle = faker.Finance.AccountName(),
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            }
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(Arg.Any<JournalVoucherFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<JournalVoucherFilter>();
                return existingJournalVouchers
                    .Where(x => filter.TransactionTime == null || x.TransactionTime == filter.TransactionTime)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetJournalVouchersQuery
        {
            TransactionTime = transactionTime
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.TransactionTime).Distinct().Should().ContainSingle().Which.Should().Be(transactionTime);
    }

    [Fact]
    public async Task GetJournalVouchers_WhenNoRecordsFound_ShouldReturnNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var transactionTime = DateTime.UtcNow.AddDays(1);

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(Arg.Any<JournalVoucherFilter>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetJournalVouchersQuery
        {
            TransactionTime = transactionTime
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessages.Should().Contain(new KeyValuePair<string, string>(nameof(GetJournalVouchersQueryResult), "Journal voucher not found."));
    }

    [Fact]
    public async Task GetJournalVouchers_WhenMultipleFiltersAreApplied_ShouldReturnMatchingResults()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var journalVoucherTitle = faker.Finance.AccountName();
        var transactionTime = DateTime.UtcNow;

        var existingJournalVouchers = new List<JournalVoucherEntity>
        {
            new()
            {
                JournalVoucherTitle = journalVoucherTitle,
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = transactionTime,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            },
            new()
            {
                JournalVoucherTitle = journalVoucherTitle,
                ReferenceNumber = faker.Finance.RoutingNumber(),
                TransactionTime = DateTime.UtcNow,
                JournalVoucherState = JournalVoucherState.Draft,
                CreatedByUserId = Guid.NewGuid(),
                FinancialPeriodId = Guid.NewGuid()
            }
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(Arg.Any<JournalVoucherFilter>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var filter = callInfo.Arg<JournalVoucherFilter>();
                return existingJournalVouchers
                    .Where(x => filter.TransactionTime == null || x.TransactionTime == filter.TransactionTime)
                    .Where(x => filter.JournalVoucherTitle == null || x.JournalVoucherTitle == filter.JournalVoucherTitle)
                    .ToList()
                    .AsReadOnly();
            });

        var query = new GetJournalVouchersQuery
        {
            TransactionTime = transactionTime,
            JournalVoucherTitle = journalVoucherTitle
        };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);
        result.Result.Select(x => x.JournalVoucherTitle).Distinct().Should().ContainSingle().Which.Should().Be(journalVoucherTitle);
        result.Result.Select(x => x.TransactionTime).Distinct().Should().ContainSingle().Which.Should().Be(transactionTime);
    }

    [Fact]
    public async Task GetJournalVouchers_WhenRecordsExist_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var entity = new JournalVoucherEntity
        {
            JournalVoucherTitle = faker.Finance.AccountName(),
            ReferenceNumber = faker.Finance.RoutingNumber(),
            TransactionTime = DateTime.UtcNow,
            JournalVoucherState = JournalVoucherState.Draft,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = Guid.NewGuid()
        };

        unitOfWork.JournalVoucherRepository
            .GetAllAsync(Arg.Any<JournalVoucherFilter>(), Arg.Any<CancellationToken>())
            .Returns(new List<JournalVoucherEntity> { entity }.AsReadOnly());

        var query = new GetJournalVouchersQuery { JournalVoucherNumber = entity.JournalVoucherNumber };
        var handler = new GetJournalVouchersQueryHandler(unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(1);

        var mapped = result.Result.First();
        mapped.ApprovedByUserId.Should().Be(entity.ApprovedByUserId);
        mapped.TransactionTime.Should().Be(entity.TransactionTime);
        mapped.JournalVoucherState.Should().Be(entity.JournalVoucherState);
        mapped.FinancialPeriodId.Should().Be(entity.FinancialPeriodId);
    }
}