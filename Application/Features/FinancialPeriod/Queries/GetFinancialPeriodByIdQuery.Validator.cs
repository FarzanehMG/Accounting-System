using FluentValidation;

namespace Application.Features.FinancialPeriod.Queries;

public class GetFinancialPeriodByIdQueryValidator : AbstractValidator<GetFinancialPeriodByIdQuery>
{
    public GetFinancialPeriodByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}