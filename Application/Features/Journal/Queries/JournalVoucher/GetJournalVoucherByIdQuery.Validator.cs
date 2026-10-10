using FluentValidation;

namespace Application.Features.Journal.Queries.JournalVoucher;

public class GetJournalVoucherByIdQueryValidator : AbstractValidator<GetJournalVoucherByIdQuery>
{
    public GetJournalVoucherByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}