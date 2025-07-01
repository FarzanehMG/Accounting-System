using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public record UpdateGeneralAccountCommand(Guid GeneralAccountId, 
    Patch<string?>? Title, 
    Patch<string?>? Description, 
    Patch<JournalEntryType>? JournalEntryType, 
    Patch<bool>? IsActive,
    Patch<Guid>? GroupAccountId);