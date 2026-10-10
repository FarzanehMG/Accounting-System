using FluentValidation;

namespace Application.Features.Accounts.Queries.DetailedAccount;

public class GetDetailedAccountByIdQueryValidator : AbstractValidator<GetDetailedAccountByIdQuery>
{
    public GetDetailedAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}