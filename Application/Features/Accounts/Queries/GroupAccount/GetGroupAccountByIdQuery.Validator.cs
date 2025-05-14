using FluentValidation;

namespace Application.Features.Accounts.Queries.GroupAccount;

public class GetGroupAccountByIdQueryValidator : AbstractValidator<GetGroupAccountByIdQuery>
{
	public GetGroupAccountByIdQueryValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty();
	}
}