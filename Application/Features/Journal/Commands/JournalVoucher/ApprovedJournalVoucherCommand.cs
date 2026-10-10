namespace Application.Features.Journal.Commands.JournalVoucher;

public record ApprovedJournalVoucherCommand
{
    public required Guid ApprovedByUserId { get; init; }
    public required Guid JournalVoucherId { get; init; }
}