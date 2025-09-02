using Domain.Enums;

namespace Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;

public record SubLedgerAccountFilter
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? GeneralLedgerAccountId { get; init; }
}