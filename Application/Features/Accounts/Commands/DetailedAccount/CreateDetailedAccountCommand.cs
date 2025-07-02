using Domain.Enums;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public record CreateDetailedAccountCommand(string Title, string? Description, JournalEntryType JournalEntryType, bool IsActive, Guid SubLedgerAccountId);