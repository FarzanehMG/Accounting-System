using FluentValidation;

namespace Application.Features.FinancialPeriod.Commands;

public class CreateFinancialPeriodCommandValidator : AbstractValidator<CreateFinancialPeriodCommand>
{
    public CreateFinancialPeriodCommandValidator()
    {
        RuleFor(x => x.FinancialPeriodTitle)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty();
    }
}