using Application.Features.Journal.Queries.JournalEntry;
using Domain.Entities;

namespace Application.Converters.Journal;

public static class JournalEntryConverter
{
    public static GetJournalEntryByIdQueryResult ToDto(this JournalEntryEntity entity)
    {
        return new GetJournalEntryByIdQueryResult
        {
            Id = entity.Id,
            JournalEntryNumber = entity.JournalEntryNumber,
            JournalEntryType = entity.JournalEntryType,
            Amount = entity.Amount,
            SubLedgerAccountId = entity.SubLedgerAccountId,
            JournalVoucherId = entity.JournalVoucherId,
            DetailedAccountId = entity.DetailedAccountId,
            Description = entity.Description
        };
    }

    public static IReadOnlyList<GetJournalEntriesQueryResult> ToDto(this IReadOnlyList<JournalEntryEntity> entity)
    {
        return entity.Select(x => new GetJournalEntriesQueryResult
            {
                Id = x.Id,
                JournalEntryNumber = x.JournalEntryNumber,
                JournalEntryType = x.JournalEntryType,
                Amount = x.Amount,
                SubLedgerAccountId = x.SubLedgerAccountId,
                JournalVoucherId = x.JournalVoucherId,
                DetailedAccountId = x.DetailedAccountId,
                Description = x.Description
            }).ToList()
            .AsReadOnly();
    }
}