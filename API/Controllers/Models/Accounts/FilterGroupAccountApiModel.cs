using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Models.Accounts;

public class FilterGroupAccountApiModel
{
    [FromQuery(Name = "Title")]
    public string? Title { get; init; }

    [FromQuery(Name = "Code")]
    public string? Code { get; init; }

    [FromQuery(Name = "IsActive")]
    public bool? IsActive { get; init; }

    [FromQuery(Name = "JournalEntryType")]
    public JournalEntryType? JournalEntryType { get; init; }
}
