using Domain.Enums;

namespace Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;

public record GeneralLedgerAccountFilter
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
    public Guid? GroupAccountId { get; init; }
}