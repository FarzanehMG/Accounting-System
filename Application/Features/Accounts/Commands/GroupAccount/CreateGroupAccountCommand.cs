using Domain.Enums;

namespace Application.Features.Accounts.Commands.GroupAccount;

public record CreateGroupAccountCommand(string Title, string? Description, JournalEntryType JournalEntryType, bool IsActive);