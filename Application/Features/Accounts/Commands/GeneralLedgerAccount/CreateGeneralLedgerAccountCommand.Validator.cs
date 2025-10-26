using Domain.Enums;
using FluentValidation;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class CreateGeneralLedgerAccountCommandValidator : AbstractValidator<CreateGeneralLedgerAccountCommand>
{
    public CreateGeneralLedgerAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.JournalEntryType)
            .Must(value => Enum.IsDefined(typeof(JournalEntryType), value))
            .WithMessage("JournalEntryType must be one of the valid enum values: Debit or Credit.");

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.GroupAccountId)
            .NotEmpty();
    }
}