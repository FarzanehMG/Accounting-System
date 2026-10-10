using FluentValidation;

namespace Application.Features.Journal.Commands.JournalVoucher;

public class ApprovedJournalVoucherCommandValidator : AbstractValidator<ApprovedJournalVoucherCommand>
{
    public ApprovedJournalVoucherCommandValidator()
    {
        RuleFor(x => x.JournalVoucherId)
            .NotEmpty();

        RuleFor(x=>x.ApprovedByUserId)
            .NotEmpty();
    }
}