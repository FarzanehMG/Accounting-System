using FluentValidation;

namespace Application.Features.Accounts.Queries.SubLedgerAccount;

public class GetSubLedgerAccountByIdQueryValidator : AbstractValidator<GetSubLedgerAccountByIdQuery>
{
    public GetSubLedgerAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}