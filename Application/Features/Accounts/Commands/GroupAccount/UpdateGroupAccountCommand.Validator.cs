using FluentValidation;

namespace Application.Features.Accounts.Commands.GroupAccount;

public class UpdateGroupAccountCommandValidator : AbstractValidator<UpdateGroupAccountCommand>
{
    public UpdateGroupAccountCommandValidator()
    {
        RuleFor(x => x.GroupAccountId)
            .NotEmpty();
    }
}