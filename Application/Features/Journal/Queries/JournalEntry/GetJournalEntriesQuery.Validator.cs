using FluentValidation;

namespace Application.Features.Journal.Queries.JournalEntry;

public class GetJournalEntriesQueryValidator : AbstractValidator<GetJournalEntriesQuery>
{
    public GetJournalEntriesQueryValidator()
    {
        RuleFor(x => x.JournalEntryNumber)
            .NotEmpty()
            .When(x => x.JournalEntryNumber is not null);

        RuleFor(x => x.JournalEntryType)
            .NotEmpty()
            .When(x => x.JournalEntryType.HasValue);

        RuleFor(x => x.Amount)
            .NotNull()
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.DetailedAccountId)
            .NotNull()
            .When(x => x.DetailedAccountId.HasValue);

        RuleFor(x => x.SubLedgerAccountId)
            .NotNull()
            .When(x => x.SubLedgerAccountId.HasValue);

        RuleFor(x => x.JournalVoucherId)
            .NotNull()
            .When(x => x.JournalVoucherId.HasValue);
    }
}