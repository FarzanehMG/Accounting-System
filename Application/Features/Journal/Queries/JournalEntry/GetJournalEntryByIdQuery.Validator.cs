using FluentValidation;

namespace Application.Features.Journal.Queries.JournalEntry;

public class GetJournalEntryByIdQueryValidator : AbstractValidator<GetJournalEntryByIdQuery>
{
    public GetJournalEntryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}