using FluentValidation;

namespace Application.Features.Journal.Commands.JournalEntry;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.JournalEntryType)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .NotEmpty()
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SubLedgerAccountId)
            .NotEmpty();

        RuleFor(x => x.JournalVoucherId)
            .NotEmpty();
    }
}