using Domain.Enums;

namespace API.Controllers.Models.Accounts;

public record FilterGroupAccountApiModel
{
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public JournalEntryType? JournalEntryType { get; init; }
}