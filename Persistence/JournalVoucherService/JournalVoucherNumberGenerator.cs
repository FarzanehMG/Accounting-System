using Application.Contracts.JournalVoucherService;
using Microsoft.EntityFrameworkCore;
using Persistence.Model;

namespace Persistence.JournalVoucherService;

public sealed class JournalVoucherNumberGenerator(AccountingDbContext dbContext) : IJournalVoucherNumberGenerator
{
    public async Task<int> GenerateAsync(Guid financialPeriodId, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

            var sequence = await dbContext.Set<JournalVoucherSequence>()
                .SingleOrDefaultAsync(x => x.FinancialPeriodId == financialPeriodId, cancellationToken);

            if (sequence is null)
            {
                sequence = new JournalVoucherSequence
                {
                    FinancialPeriodId = financialPeriodId,
                    CurrentNumber = 1
                };

                dbContext.Add(sequence);
            }
            else
            {
                sequence.CurrentNumber++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return sequence.CurrentNumber;
        });
    }
}
