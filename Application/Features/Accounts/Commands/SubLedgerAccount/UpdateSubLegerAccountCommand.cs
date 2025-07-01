using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public record UpdateSubLegerAccountCommand(Guid SubLegerAccountId,
    Patch<string?>? Title,
    Patch<string?>? Description,
    Patch<JournalEntryType>? JournalEntryType,
    Patch<bool>? IsActive);