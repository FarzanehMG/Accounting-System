using Application.Common;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public record UpdateSubLedgerAccountCommand
{
    public required Guid SubLegerAccountId { get; init; }
    public Patch<string>? Title { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<bool>? IsActive { get; init; }
    public Patch<string>? Code { get; init; }
}