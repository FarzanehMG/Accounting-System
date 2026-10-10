using Domain.Enums;
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

        RuleFor(x => x.JournalEntryType)
            .Must(value => Enum.IsDefined(typeof(JournalEntryType), value))
            .WithMessage("JournalEntryType must be one of the valid enum values: Debit or Credit.");
    }
}