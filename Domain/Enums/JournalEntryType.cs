using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JournalEntryType
{
    Debit = 1,
    Credit = 2,
}