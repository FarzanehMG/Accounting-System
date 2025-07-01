using Domain.Enums;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public record CreateGeneralAccountCommand(string Title, string? Description, JournalEntryType JournalEntryType, bool IsActive, Guid GroupAccountId);