using FluentValidation;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLedgerAccountByTitleQueryValidator : AbstractValidator<GetSubLedgerAccountByTitleQuery>
{
    public GetSubLedgerAccountByTitleQueryValidator()
    {
        RuleFor(x => x.AccountTitle)
            .NotEmpty();
    }
}