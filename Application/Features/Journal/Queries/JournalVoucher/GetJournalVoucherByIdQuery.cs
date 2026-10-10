namespace Application.Features.Journal.Queries.JournalVoucher;

public record GetJournalVoucherByIdQuery
{
    public required Guid Id { get; init; }
}