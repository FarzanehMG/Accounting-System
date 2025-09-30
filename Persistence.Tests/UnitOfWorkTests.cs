using Domain.Entities.Accounts;
using Domain.Enums;
using FluentAssertions;
using Persistence.Repositories.Common;
using Xunit.Abstractions;

namespace Persistence.Tests;

public class UnitOfWorkTests : IClassFixture<PersistenceTestSetup>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests(PersistenceTestSetup setup, ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _unitOfWork = setup.UnitOfWork;
    }

    [Fact]
    public async Task Adding_New_GroupAccount_Should_Save_To_Database()
    {
        // Arrange
        var groupAccount = new GroupAccountEntity
        {
            AccountTitle = "Test Group Account",
            AccountCode = "1000",
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };

        // Act
        await _unitOfWork.GroupAccountRepository.CreateAsync(groupAccount);
        await _unitOfWork.CommitAsync();

        var savedAccount = await _unitOfWork.GroupAccountRepository.FindActiveAccountByTitleAndTypeAsync(groupAccount.AccountTitle, groupAccount.JournalEntryType);

        // Assert
        savedAccount.Should().NotBeNull();
        savedAccount.AccountTitle.Should().Be("Test Group Account");
        savedAccount.IsActive.Should().BeTrue();
        savedAccount.JournalEntryType.Should().Be(JournalEntryType.Debit);
    }

    [Fact]
    public async Task Getting_GroupAccount_By_Id_Should_Success()
    {
        // Arrange
        var groupAccount = new GroupAccountEntity
        {
            AccountTitle = "Test Group Account 1",
            AccountCode = "1001",
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };

        // Act
        await _unitOfWork.GroupAccountRepository.CreateAsync(groupAccount);
        await _unitOfWork.CommitAsync();

        var groupAccountById = await _unitOfWork.GroupAccountRepository.GetByIdAsync(groupAccount.Id);

        // Assert
        groupAccountById.Should().NotBeNull();
    }

    [Fact]
    public async Task GroupAccount_Added_Date_Should_Have_Value()
    {
        // Arrange
        var groupAccount = new GroupAccountEntity
        {
            AccountTitle = "Test Group Account 2",
            AccountCode = "1002",
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };

        // Act
        await _unitOfWork.GroupAccountRepository.CreateAsync(groupAccount);
        await _unitOfWork.CommitAsync();

        var groupAccountById = await _unitOfWork.GroupAccountRepository.GetByIdAsync(groupAccount.Id);

        // Assert
        groupAccountById!.CreatedDate.Should().BeMoreThan(TimeSpan.MinValue);
        _outputHelper.WriteLine($"Current Added Group Account Date {groupAccountById.CreatedDate}");
    }

    [Fact]
    public async Task GroupAccount_Modified_Date_Should_Not_Have_Value_When_Added()
    {
        // Arrange
        var groupAccount = new GroupAccountEntity
        {
            AccountTitle = "Test Group Account 3",
            AccountCode = "1003",
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };

        // Act
        await _unitOfWork.GroupAccountRepository.CreateAsync(groupAccount);
        await _unitOfWork.CommitAsync();

        var groupAccountById = await _unitOfWork.GroupAccountRepository.GetByIdAsync(groupAccount.Id);

        // Assert
        groupAccountById!.ModifiedDate.Should().BeNull();
    }

    [Fact]
    public async Task GroupAccount_Modified_Date_Should_Have_Value_When_Updated()
    {
        // Arrange
        var groupAccount = new GroupAccountEntity
        {
            AccountTitle = "Test Group Account 4",
            AccountCode = "1004",
            JournalEntryType = JournalEntryType.Debit,
            IsActive = true
        };

        // Act
        await _unitOfWork.GroupAccountRepository.CreateAsync(groupAccount);
        await _unitOfWork.CommitAsync();

        var groupAccountById = await _unitOfWork.GroupAccountRepository.GetByIdAsync(groupAccount.Id);

        groupAccountById!.AccountTitle = "Test Edited Group Account 4";
        await _unitOfWork.CommitAsync();

        var newGroupAccountById = await _unitOfWork.GroupAccountRepository.GetByIdAsync(groupAccount.Id);

        // Assert
        newGroupAccountById!.ModifiedDate.Should().BeMoreThan(TimeSpan.MinValue);
        _outputHelper.WriteLine($"Current Updated Group Account Date {newGroupAccountById.ModifiedDate}");
    }
}
