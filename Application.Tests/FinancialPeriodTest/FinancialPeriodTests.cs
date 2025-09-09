using Application.Common;
using Application.Features.FinancialPeriod.Commands;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Application.Tests.FinancialPeriodTest;

public class FinancialPeriodTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = "",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(100)
        };

        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenStartDateIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.MinValue,
            EndDate = DateTime.UtcNow.AddDays(100)
        };

        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenEndDateIsEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.MinValue
        };

        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenEndDateLessThanStartDate()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-10)
        };

        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("StartDate")
            .WhoseValue.Should().Contain("The end date must be later than the start date of the financial period.");

        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenPeriodOverlapWithExistingFinancialPeriod()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow;

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = endDate
        };
        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = Guid.NewGuid(),
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository
            .GetOverlappingFinancialPeriodAsync(CancellationToken.None, startDate, endDate)
            .Returns(existingFinancialPeriodEntity);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("EndDate")
            .WhoseValue.Should().Contain("The selected period overlaps with an existing financial period.");

        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_ShouldFail_WhenHaveActiveExistingFinancialPeriod()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow;

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = endDate
        };
        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = Guid.NewGuid(),
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository
            .GetActiveFinancialPeriodAsync(CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("IsActive")
            .WhoseValue.Should().Contain("You must close the current active financial period before creating a new one.");

        testOutputHelper.WriteLineOperationResultErrors(result);
    }

    [Fact]
    public async Task CreateFinancialPeriodHandler_WhenFinancialPeriodCommandIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };
        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task When_Validation_Fails_Should_Not_Call_Commit()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = "",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };
        var handler = new CreateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        await handler.Handle(createFinancialPeriodCommand, CancellationToken.None);

        // Assert
        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldFail_WhenFinancialPeriodIdNotFound()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers =
            [
                new JournalVoucherEntity
                {
                    JournalVoucherTitle = faker.Finance.AccountName(),
                    JournalVoucherNumber = faker.Finance.Account(),
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TransactionTime = DateTime.UtcNow,
                    Description = faker.Finance.AccountName(),
                    JournalVoucherState = JournalVoucherState.Draft,
                    CompanyFinancialPeriodId = Guid.NewGuid()
                }
            ],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = Guid.NewGuid()
        };

        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("UpdateFinancialPeriodCommand")
            .WhoseValue.Should().Contain("Financial period not found.");
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldSuccess_WhenTitleIsNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers =
            [
                new JournalVoucherEntity
                {
                    JournalVoucherTitle = faker.Finance.AccountName(),
                    JournalVoucherNumber = faker.Finance.Account(),
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TransactionTime = DateTime.UtcNow,
                    Description = faker.Finance.AccountName(),
                    JournalVoucherState = JournalVoucherState.Draft,
                    CompanyFinancialPeriodId = Guid.NewGuid()
                }
            ],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        unitOfWork.HasChanges(existingFinancialPeriodEntity)
            .Returns(true);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            FinancialPeriodTitle = new Patch<string>(faker.Finance.AccountName())
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingFinancialPeriodEntity.FinancialPeriodTitle.Should().Be(updateFinancialPeriodCommand.FinancialPeriodTitle);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldFail_WhenJournalVoucherAndStartDateNotEmpty()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers =
            [
                new JournalVoucherEntity
                {
                    JournalVoucherTitle = faker.Finance.AccountName(),
                    JournalVoucherNumber = faker.Finance.Account(),
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TransactionTime = DateTime.UtcNow,
                    Description = faker.Finance.AccountName(),
                    JournalVoucherState = JournalVoucherState.Draft,
                    CompanyFinancialPeriodId = Guid.NewGuid()
                }
            ],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            StartDate = new Patch<DateTime>(DateTime.UtcNow.AddDays(2))
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("StartDate")
            .WhoseValue.Should().Contain("Cannot change StartDate because some journal entries already exist.");
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldFail_WhenEndDateNotLaterThatStartDate()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers =
            [
                new JournalVoucherEntity
                {
                    JournalVoucherTitle = faker.Finance.AccountName(),
                    JournalVoucherNumber = faker.Finance.Account(),
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TransactionTime = DateTime.UtcNow,
                    Description = faker.Finance.AccountName(),
                    JournalVoucherState = JournalVoucherState.Draft,
                    CompanyFinancialPeriodId = Guid.NewGuid()
                }
            ],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            EndDate = new Patch<DateTime>(startDate.AddDays(-2))
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("EndDate")
            .WhoseValue.Should().Contain("End date must be later than start date.");
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldFail_WhenOverlapFinancialPeriod()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers =
            [
                new JournalVoucherEntity
                {
                    JournalVoucherTitle = faker.Finance.AccountName(),
                    JournalVoucherNumber = faker.Finance.Account(),
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TransactionTime = DateTime.UtcNow,
                    Description = faker.Finance.AccountName(),
                    JournalVoucherState = JournalVoucherState.Draft,
                    CompanyFinancialPeriodId = Guid.NewGuid()
                }
            ],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        unitOfWork.FinancialPeriodRepository.GetOverlappingFinancialPeriodAsync(CancellationToken.None, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(existingFinancialPeriodEntity);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            EndDate = new Patch<DateTime>(startDate.AddDays(5))
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessages.Should().ContainKey("Date")
            .WhoseValue.Should().Contain("The selected period overlaps with an existing financial period.");
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldSucceed_WhenMultipleFieldsAreUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers = [],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        unitOfWork.HasChanges(Arg.Any<FinancialPeriodEntity>())
            .Returns(true);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            StartDate = new Patch<DateTime>(DateTime.UtcNow.AddDays(8)),
            EndDate = new Patch<DateTime>(startDate.AddDays(16))
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingFinancialPeriodEntity.StartDate.Should().Be(updateFinancialPeriodCommand.StartDate);
        existingFinancialPeriodEntity.EndDate.Should().Be(updateFinancialPeriodCommand.EndDate);

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldUpdateModifiedDate_WhenFinancialPeriodIsUpdated()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers = [],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = oldModifiedDate
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        unitOfWork.HasChanges(Arg.Any<FinancialPeriodEntity>())
            .Returns(true);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId,
            StartDate = new Patch<DateTime>(DateTime.UtcNow.AddDays(8)),
            EndDate = new Patch<DateTime>(startDate.AddDays(16))
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingFinancialPeriodEntity.StartDate.Should().Be(updateFinancialPeriodCommand.StartDate);
        existingFinancialPeriodEntity.EndDate.Should().Be(updateFinancialPeriodCommand.EndDate);
        existingFinancialPeriodEntity.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        await unitOfWork.Received(1)
            .CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateFinancialPeriodHandler_ShouldSucceed_WhenNoFieldsAreChanged()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var faker = new Faker();
        var financialPeriodId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var oldModifiedDate = DateTime.UtcNow.AddDays(-5);

        var existingFinancialPeriodEntity = new FinancialPeriodEntity
        {
            Id = financialPeriodId,
            FinancialPeriodTitle = faker.Finance.AccountName(),
            StartDate = startDate,
            EndDate = DateTime.UtcNow,
            IsActive = true,
            JournalVouchers = [],
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = oldModifiedDate
        };
        unitOfWork.FinancialPeriodRepository.GetFinancialPeriodByIdAsync(financialPeriodId, CancellationToken.None)
            .Returns(existingFinancialPeriodEntity);

        unitOfWork.HasChanges(Arg.Any<FinancialPeriodEntity>())
            .Returns(false);

        var updateFinancialPeriodCommand = new UpdateFinancialPeriodCommand
        {
            FinancialPeriodId = financialPeriodId
        };
        var handler = new UpdateFinancialPeriodCommandHandler(unitOfWork);

        // Act
        var result = await handler.Handle(updateFinancialPeriodCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingFinancialPeriodEntity.ModifiedDate.Should().Be(oldModifiedDate);

        await unitOfWork.DidNotReceive()
            .CommitAsync(CancellationToken.None);
    }
}