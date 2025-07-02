using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public record UpdateDetailedAccountCommand(Guid DetailedAccountId,
    Patch<string?>? Title,
    Patch<string?>? Description,
    Patch<JournalEntryType>? JournalEntryType,
    Patch<bool>? IsActive,
    Patch<Guid>? SubLedgerAccountId);