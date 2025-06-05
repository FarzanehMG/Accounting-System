namespace Application.Features.Accounts.Commands.GroupAccount;

public record UpdateGroupAccountCommand(Guid GroupAccountId, string? Title, string? Description);