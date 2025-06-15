using FluentValidation;

namespace Application.Features.Accounts.Commands.GeneralAccount;

public class UpdateGeneralAccountCommandValidator : AbstractValidator<UpdateGeneralAccountCommand>
{
    public UpdateGeneralAccountCommandValidator()
    {
        RuleFor(x => x.GeneralAccountId)
            .NotEmpty();
    }
}