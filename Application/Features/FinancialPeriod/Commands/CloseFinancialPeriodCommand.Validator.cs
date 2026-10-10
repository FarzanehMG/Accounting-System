using FluentValidation;

namespace Application.Features.FinancialPeriod.Commands;

public class CloseFinancialPeriodCommandValidator : AbstractValidator<CloseFinancialPeriodCommand>
{
    public CloseFinancialPeriodCommandValidator()
    {
        RuleFor(x => x.FinancialPeriodId)
            .NotEmpty();

        RuleFor(x => x.NewPeriodTitle)
            .NotEmpty();

        RuleFor(x => x.NewStartDate)
            .NotEmpty();

        RuleFor(x => x.NewEndDate)
            .NotEmpty();

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty();
    }
}