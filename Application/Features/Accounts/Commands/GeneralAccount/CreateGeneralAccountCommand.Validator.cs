using FluentValidation;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public class CreateGeneralAccountCommandValidator : AbstractValidator<CreateGeneralAccountCommand>
{
    public CreateGeneralAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x=>x.JournalEntryType)
            .NotEmpty();
    }
}