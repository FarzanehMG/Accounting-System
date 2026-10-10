using Domain.Enums;

namespace Application.Features.Accounts.Queries.DetailedAccount.Dtos;

public record DetailedAccountFilter
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? SubLedgerAccountId { get; init; }
}