using Domain.Enums;

namespace Application.Features.Accounts.Queries.GroupAccount.Dtos;

public record GroupAccountFilter
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
}