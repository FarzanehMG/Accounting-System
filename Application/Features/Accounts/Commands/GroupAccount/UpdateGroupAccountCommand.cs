using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.GroupAccount;

public record UpdateGroupAccountCommand
{
    public Guid GroupAccountId { get; init; }
    public Patch<string?>? Title { get; init; }
    public Patch<string?>? Description { get; init; }
    public Patch<JournalEntryType>? JournalEntryType { get; init; }
    public Patch<bool>? IsActive { get; init; }
}
