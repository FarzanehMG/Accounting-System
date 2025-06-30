using FluentValidation;

namespace Application.Features.Accounts.Queries.GeneralAccount;

public class GetGeneralAccountByTitleQueryValidator : AbstractValidator<GetGeneralAccountByTitleQuery>
{
    public GetGeneralAccountByTitleQueryValidator()
    {
        RuleFor(x => x.AccountTitle)
            .NotEmpty();
    }
}