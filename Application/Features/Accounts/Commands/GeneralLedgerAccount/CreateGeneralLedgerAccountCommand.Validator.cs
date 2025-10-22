using FluentValidation;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class CreateGeneralLedgerAccountCommandValidator : AbstractValidator<CreateGeneralLedgerAccountCommand>
{
    public CreateGeneralLedgerAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x=>x.JournalEntryType)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.GroupAccountId)
            .NotEmpty();
    }
}