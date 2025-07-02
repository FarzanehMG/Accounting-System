using FluentValidation;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountByTitleQueryValidator : AbstractValidator<GetDetailedAccountByTitleQuery>
{
    public GetDetailedAccountByTitleQueryValidator()
    {
        RuleFor(x => x.AccountTitle)
            .NotEmpty();
    }
}