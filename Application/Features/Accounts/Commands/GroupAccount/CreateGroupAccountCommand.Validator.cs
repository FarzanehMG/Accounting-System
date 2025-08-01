using FluentValidation;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class CreateGroupAccountCommandValidator : AbstractValidator<CreateGroupAccountCommand>
{
	public CreateGroupAccountCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.IsActive)
            .NotEmpty();

        RuleFor(x => x.JournalEntryType)
            .NotEmpty();
    }
}