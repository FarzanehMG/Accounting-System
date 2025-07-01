using FluentValidation;

namespace Application.Features.Accounts.Commands.SubLedgerAccount;

public class UpdateSubLegerAccountCommandValidator : AbstractValidator<UpdateSubLegerAccountCommand>
{
    public UpdateSubLegerAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}