using FluentValidation;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountByTitleQueryValidator : AbstractValidator<GetGroupAccountByTitleQuery>
{
    public GetGroupAccountByTitleQueryValidator()
    {
        RuleFor(x => x.AccountTitle)
            .NotEmpty();
    }
}