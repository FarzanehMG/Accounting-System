using Domain.Enums;

namespace Application.Features.Accounts.Commands.GroupAccount;

public record CreateGroupAccountCommand
{
    public required string Title { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
    public required JournalEntryType JournalEntryType { get; init; }
    public required bool IsActive { get; init; }
}