using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JournalVoucherState
{
    Draft = 1,        // Temporary
    Reviewed = 2,     // Approved
    Posted = 3,       // Permanent
    Reversed = 4     
}