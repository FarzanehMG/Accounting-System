using FluentValidation;

namespace Application.Features.FinancialPeriod.Queries;

public class GetFinancialPeriodsQueryValidator : AbstractValidator<GetFinancialPeriodsQuery>
{
    public GetFinancialPeriodsQueryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .When(x => x.StartDate is not null);

        RuleFor(x => x.EndDate)
            .NotNull()
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull()
            .When(x => x.IsActive.HasValue);
    }
}