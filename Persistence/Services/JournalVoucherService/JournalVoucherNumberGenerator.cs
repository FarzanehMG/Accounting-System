using Application.Contracts.JournalVoucherService;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services.JournalVoucherService;

public class JournalVoucherNumberGenerator(AccountingDbContext db) : IJournalVoucherNumberGenerator
{
    public async Task<int> GenerateAsync(Guid financialPeriodId, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Must run inside transaction.");

        // Step 1: ensure sequence exists (SAFE initialization pattern)
        var initSql = @"
            IF NOT EXISTS (
                SELECT 1
                FROM JournalVoucherSequences WITH (UPDLOCK, HOLDLOCK)
                WHERE FinancialPeriodId = {0}
            )
            BEGIN
                INSERT INTO JournalVoucherSequences (FinancialPeriodId, CurrentNumber)
                VALUES ({0}, 0)
            END";

        await db.Database.ExecuteSqlRawAsync(initSql, financialPeriodId);

        // Step 2: atomic increment
        var updateSql = @"
            UPDATE JournalVoucherSequences WITH (ROWLOCK, UPDLOCK)
            SET CurrentNumber = CurrentNumber + 1
            OUTPUT INSERTED.CurrentNumber
            WHERE FinancialPeriodId = {0};";

        var result = await db.Database
            .SqlQueryRaw<int>(updateSql, financialPeriodId)
            .ToListAsync(ct);

        if (result.Count == 0)
            throw new InvalidOperationException("Sequence generation failed.");

        return result[0];
    }
}