using FluentValidation;

namespace Application.Features.Accounts.Queries.GeneralAccount;

public class GetGeneralAccountByIdQueryValidator : AbstractValidator<GetGeneralAccountByIdQuery>
{
    public GetGeneralAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}