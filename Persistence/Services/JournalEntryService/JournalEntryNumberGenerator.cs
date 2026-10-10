using Application.Contracts.JournalEntryService;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.JournalEntryService;

public class JournalEntryNumberGenerator(AccountingDbContext db) : IJournalEntryNumberGenerator
{
    public async Task<int> GenerateAsync(Guid voucherId, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Must run inside transaction.");

        // safe initialization (no race condition via constraint reliance)
        var initSql = @"
            IF NOT EXISTS (
                SELECT 1
                FROM JournalEntrySequences WITH (UPDLOCK, HOLDLOCK)
                WHERE JournalVoucherId = {0}
            )
            BEGIN
                INSERT INTO JournalEntrySequences (JournalVoucherId, CurrentNumber)
                VALUES ({0}, 0)
            END";

        await db.Database.ExecuteSqlRawAsync(initSql, voucherId);

        // atomic increment (single deterministic output)
        var updateSql = @"
            UPDATE JournalEntrySequences WITH (ROWLOCK, UPDLOCK)
            SET CurrentNumber = CurrentNumber + 1
            OUTPUT INSERTED.CurrentNumber
            WHERE JournalVoucherId = {0};";

        var result = await db.Database
            .SqlQueryRaw<int>(updateSql, voucherId)
            .ToListAsync(cancellationToken);

        if (result.Count != 1)
            throw new InvalidOperationException("Sequence generation failed.");

        return result[0];
    }
}