using Domain.Enums;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public record CreateSubLedgerAccountCommand(string Title, string? Description, JournalEntryType JournalEntryType, bool IsActive, Guid GeneralLedgerAccountId);