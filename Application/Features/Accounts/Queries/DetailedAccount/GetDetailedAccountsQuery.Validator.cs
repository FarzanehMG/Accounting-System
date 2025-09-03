using FluentValidation;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountsQueryValidator : AbstractValidator<GetDetailedAccountsQuery>
{
    public GetDetailedAccountsQueryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null);

        RuleFor(x => x.Code)
            .NotEmpty()
            .When(x => x.Code is not null);

        RuleFor(x => x.JournalEntryType)
            .NotNull()
            .When(x => x.JournalEntryType.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull()
            .When(x => x.IsActive.HasValue);

        RuleFor(x => x.SubLedgerAccountId)
            .NotNull()
            .When(x => x.SubLedgerAccountId.HasValue);
    }
}