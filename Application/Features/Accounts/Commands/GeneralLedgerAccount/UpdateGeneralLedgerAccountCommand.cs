using Application.Common;
using Domain.Enums;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public record UpdateGeneralLedgerAccountCommand
{
    public required Guid GeneralLedgerAccountId { get; init; }
    public Patch<string>? Title { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<JournalEntryType>? JournalEntryType { get; init; }
    public Patch<bool>? IsActive { get; init; }
    public Patch<string>? Code { get; init; }
    public Patch<Guid>? GroupAccountId { get; init; }
}