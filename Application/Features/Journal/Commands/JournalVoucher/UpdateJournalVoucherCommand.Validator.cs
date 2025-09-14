using FluentValidation;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class UpdateJournalVoucherCommandValidator : AbstractValidator<UpdateJournalVoucherCommand>
{
    public UpdateJournalVoucherCommandValidator()
    {
        RuleFor(x => x.JournalVoucherId)
            .NotEmpty();
    }
}