using FluentValidation;

namespace Application.Features.FinancialPeriod.Commands;

public class UpdateFinancialPeriodCommandValidator : AbstractValidator<UpdateFinancialPeriodCommand>
{
    public UpdateFinancialPeriodCommandValidator()
    {
        RuleFor(x => x.FinancialPeriodId)
            .NotEmpty();
    }
}