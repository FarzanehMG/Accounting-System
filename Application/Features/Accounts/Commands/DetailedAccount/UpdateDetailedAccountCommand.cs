using Application.Common;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public record UpdateDetailedAccountCommand
{
    public required Guid DetailedAccountId { get; init; }
    public Patch<string>? Title { get; init; }
    public Patch<string>? Description { get; init; }
    public Patch<bool>? IsActive { get; init; }
    public Patch<string>? Code { get; init; }
}