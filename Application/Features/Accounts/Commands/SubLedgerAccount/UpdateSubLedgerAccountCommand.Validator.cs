using FluentValidation;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class UpdateSubLegerAccountCommandValidator : AbstractValidator<UpdateSubLedgerAccountCommand>
{
    public UpdateSubLegerAccountCommandValidator()
    {
        RuleFor(x => x.SubLedgerAccountId)
            .NotEmpty();
    }
}