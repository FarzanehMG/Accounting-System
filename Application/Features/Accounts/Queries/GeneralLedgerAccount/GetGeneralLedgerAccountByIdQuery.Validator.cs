using FluentValidation;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount;

public class GetGeneralLedgerAccountByIdQueryValidator : AbstractValidator<GetGeneralLedgerAccountByIdQuery>
{
    public GetGeneralLedgerAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}