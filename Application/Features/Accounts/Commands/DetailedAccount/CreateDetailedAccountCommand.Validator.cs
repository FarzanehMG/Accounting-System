using FluentValidation;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class CreateDetailedAccountCommandValidator : AbstractValidator<CreateDetailedAccountCommand>
{
    public CreateDetailedAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.IsActive)
            .NotEmpty();

        RuleFor(x => x.JournalEntryType)
            .NotEmpty();

        RuleFor(x => x.SubLedgerAccountId)
            .NotEmpty();
    }
}