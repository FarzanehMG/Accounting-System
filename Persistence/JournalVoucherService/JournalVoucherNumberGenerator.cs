using Application.Contracts.JournalVoucherService;
using Microsoft.EntityFrameworkCore;

namespace Persistence.JournalVoucherService;

public sealed class JournalVoucherNumberGenerator(AccountingDbContext dbContext) : IJournalVoucherNumberGenerator
{
    public async Task<int> GenerateAsync(Guid financialPeriodId, CancellationToken cancellationToken)
    {
        var sql = @"
            SET NOCOUNT ON;
            MERGE INTO JournalVoucherSequences AS Target
            USING (SELECT @p0 AS FinancialPeriodId) AS Source
            ON Target.FinancialPeriodId = Source.FinancialPeriodId
            WHEN MATCHED THEN
                UPDATE SET CurrentNumber = Target.CurrentNumber + 1
            WHEN NOT MATCHED THEN
                INSERT (FinancialPeriodId, CurrentNumber)
                VALUES (Source.FinancialPeriodId, 1)
            OUTPUT INSERTED.CurrentNumber;";

        var result = await dbContext.Database
            .SqlQueryRaw<int>(sql, financialPeriodId)
            .ToListAsync(cancellationToken);

        return result.First();
    }
}
