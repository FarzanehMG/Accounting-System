using FluentValidation;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class CreateJournalVoucherCommandValidator : AbstractValidator<CreateJournalVoucherCommand>
{
    public CreateJournalVoucherCommandValidator()
    {
        RuleFor(x => x.JournalVoucherTitle)
            .NotEmpty();

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty();

        RuleFor(x => x.TransactionTime)
            .NotEmpty();

        RuleFor(x => x.FinancialPeriodId)
            .NotEmpty();

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty();
    }
}