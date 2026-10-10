using Application.Common;

namespace Application.Features.Journal.Commands.JournalVoucher;

public record UpdateJournalVoucherCommand
{
    public required Guid JournalVoucherId { get; init; }
    public Patch<string>? JournalVoucherTitle { get; init; }
    public Patch<string>? ReferenceNumber { get; init; }
    public Patch<DateTime>? TransactionTime { get; init; }
    public Patch<string>? Description { get; init; }
}