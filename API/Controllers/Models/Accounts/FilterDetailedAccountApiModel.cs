using Domain.Enums;

namespace API.Controllers.Models.Accounts;

public record FilterDetailedAccountApiModel
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? SubLedgerAccountId { get; init; }
}