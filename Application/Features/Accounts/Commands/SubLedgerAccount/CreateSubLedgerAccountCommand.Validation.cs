using FluentValidation;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class CreateSubLedgerAccountCommandValidator : AbstractValidator<CreateSubLedgerAccountCommand>
{
    public CreateSubLedgerAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.JournalEntryType)
            .NotEmpty();

        RuleFor(x=>x.IsActive)
            .NotEmpty();
    }
}