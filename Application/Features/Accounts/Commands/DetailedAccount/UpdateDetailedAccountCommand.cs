using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public record UpdateDetailedAccountCommand
{
    public required Guid DetailedAccountId { get; init; }
    public Patch<Guid>? SubLedgerAccountId { get; init; }
    public Patch<JournalEntryType>? JournalEntryType { get; init; }
    public Patch<string>? Title { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<bool>? IsActive { get; init; }
    public Patch<string>? Code { get; init; }
}