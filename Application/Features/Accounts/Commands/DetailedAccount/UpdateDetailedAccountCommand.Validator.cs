using FluentValidation;

namespace Application.Features.Accounts.Commands.DetailedAccount;

public class UpdateDetailedAccountCommandValidator : AbstractValidator<UpdateDetailedAccountCommand>
{
    public UpdateDetailedAccountCommandValidator()
    {
        RuleFor(x => x.DetailedAccountId)
            .NotEmpty();
    }
}