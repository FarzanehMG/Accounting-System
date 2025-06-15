using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.GroupAccount;

public record UpdateGroupAccountCommand(Guid GroupAccountId, 
    Patch<string?>? Title, 
    Patch<string?>? Description, 
    Patch<JournalEntryType>? JournalEntryType, 
    Patch<bool>? IsActive);