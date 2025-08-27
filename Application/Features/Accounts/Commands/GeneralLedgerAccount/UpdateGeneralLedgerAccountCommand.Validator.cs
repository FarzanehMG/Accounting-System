using FluentValidation;

namespace Application.Features.Accounts.Commands.GeneralLedgerAccount;

public class UpdateGeneralLedgerAccountCommandValidator : AbstractValidator<UpdateGeneralLedgerAccountCommand>
{
    public UpdateGeneralLedgerAccountCommandValidator()
    {
        RuleFor(x => x.GeneralLedgerAccountId)
            .NotEmpty();
    }
}