using Application.Features.FinancialPeriod.Commands;
using Application.Repositories.Common;
using Application.Tests.Extensions;
using Bogus;
using Domain.Entities;
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
            .GetOverlappingFinancialPeriodAsync(startDate, endDate, CancellationToken.None)
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
        var faker = new Faker();

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
}