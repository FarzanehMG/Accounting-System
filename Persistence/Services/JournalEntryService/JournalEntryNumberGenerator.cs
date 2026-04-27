using Application.Contracts.JournalEntryService;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.JournalEntryService;

public class JournalEntryNumberGenerator(AccountingDbContext db) : IJournalEntryNumberGenerator
{
    public async Task<int> GenerateAsync(Guid voucherId, CancellationToken cancellationToken)
    {
        var sql = @"
            UPDATE JournalEntrySequences WITH (UPDLOCK, HOLDLOCK)
            SET CurrentNumber = CurrentNumber + 1
            OUTPUT INSERTED.CurrentNumber
            WHERE JournalVoucherId = {0};

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO JournalEntrySequences (JournalVoucherId, CurrentNumber)
                VALUES ({0}, 1);

                SELECT 1;
            END";

        var result = await db.Database
            .SqlQueryRaw<int>(sql, voucherId)
            .ToListAsync(cancellationToken);

        return result[0];
    }
}