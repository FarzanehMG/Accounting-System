using FluentValidation;

namespace Application.Features.Journal.Queries.JournalVoucher;

public class GetJournalVouchersQueryValidator : AbstractValidator<GetJournalVouchersQuery>
{
    public GetJournalVouchersQueryValidator()
    {
        RuleFor(x => x.JournalVoucherTitle)
            .NotEmpty()
            .When(x => x.JournalVoucherTitle is not null);

        RuleFor(x => x.JournalVoucherNumber)
            .NotEmpty()
            .When(x => x.JournalVoucherNumber is not null);

        RuleFor(x => x.ReferenceNumber)
            .NotNull()
            .When(x => x.ReferenceNumber is not null);

        RuleFor(x => x.TransactionTime)
            .NotNull()
            .When(x => x.TransactionTime.HasValue);

        RuleFor(x => x.JournalVoucherState)
            .NotNull()
            .When(x => x.JournalVoucherState.HasValue);

        RuleFor(x => x.CreatedByUserId)
            .NotNull()
            .When(x => x.CreatedByUserId.HasValue);

        RuleFor(x => x.ApprovedByUserId)
            .NotNull()
            .When(x => x.ApprovedByUserId.HasValue);

        RuleFor(x => x.ApprovedTime)
            .NotNull()
            .When(x => x.ApprovedTime.HasValue);

        RuleFor(x => x.FinancialPeriodId)
            .NotNull()
            .When(x => x.FinancialPeriodId.HasValue);
    }
}