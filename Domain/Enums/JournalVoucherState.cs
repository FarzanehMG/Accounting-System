using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JournalVoucherState
{
    Draft = 1,
    Approved = 2,
    Rejected = 3,
}