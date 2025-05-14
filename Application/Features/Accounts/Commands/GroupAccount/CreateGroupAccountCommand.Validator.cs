using FluentValidation;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class CreateGroupAccountCommandValidator : AbstractValidator<CreateGroupAccountCommand>
{
	public CreateGroupAccountCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty();
	}
}