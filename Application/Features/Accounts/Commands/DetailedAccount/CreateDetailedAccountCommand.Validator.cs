using Domain.Enums;
using FluentValidation;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class CreateDetailedAccountCommandValidator : AbstractValidator<CreateDetailedAccountCommand>
{
    public CreateDetailedAccountCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.JournalEntryType)
            .Must(value => Enum.IsDefined(typeof(JournalEntryType), value))
            .WithMessage("JournalEntryType must be one of the valid enum values: Debit or Credit.");

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.SubLedgerAccountId)
            .NotEmpty();
    }
}