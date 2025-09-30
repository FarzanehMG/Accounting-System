using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Application.Repositories.JournalRepositories;
using Domain.Entities.Journal;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Journal;

internal sealed class JournalVoucherRepository(AccountingDbContext dbContext) : BaseRepository<JournalVoucherEntity>(dbContext), IJournalVoucherRepository
{
    public async Task CreateAsync(JournalVoucherEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<JournalVoucherEntity?> GetLastJournalVoucherAsync(Guid financialPeriodId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking
            .Where(x => x.FinancialPeriodId == financialPeriodId)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<JournalVoucherEntity?> GetByIdAsync(Guid journalVoucherId, CancellationToken cancellationToken = default)
    {
        return await Table.FirstOrDefaultAsync(x => x.Id == journalVoucherId, cancellationToken);
    }

    public async Task<JournalVoucherEntity?> GetByIdWithEntriesAsync(Guid journalVoucherId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking
            .Include(x => x.JournalEntries)
            .FirstOrDefaultAsync(x => x.Id == journalVoucherId, cancellationToken);
    }

    public async Task<IReadOnlyList<JournalVoucherEntity>> GetAllAsync(JournalVoucherFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking;

        if (!string.IsNullOrEmpty(filter.JournalVoucherTitle))
            query = query.Where(x => x.JournalVoucherTitle.Equals(filter.JournalVoucherTitle));

        if (!string.IsNullOrEmpty(filter.JournalVoucherNumber))
            query = query.Where(x => x.JournalVoucherNumber.Equals(filter.JournalVoucherNumber));

        if (!string.IsNullOrEmpty(filter.ReferenceNumber))
            query = query.Where(x => x.ReferenceNumber.Equals(filter.ReferenceNumber));

        if (filter.JournalVoucherState.HasValue)
            query = query.Where(x => x.JournalVoucherState == filter.JournalVoucherState.Value);

        if (filter.TransactionTime.HasValue)
            query = query.Where(x => x.TransactionTime == filter.TransactionTime.Value.Date);

        if (filter.ApprovedTime.HasValue)
            query = query.Where(x => x.ApprovedTime == filter.ApprovedTime.Value.Date);

        if (filter.FinancialPeriodId.HasValue)
            query = query.Where(x => x.FinancialPeriodId == filter.FinancialPeriodId.Value);

        if (filter.CreatedByUserId.HasValue)
            query = query.Where(x => x.CreatedByUserId == filter.CreatedByUserId.Value);

        if (filter.ApprovedByUserId.HasValue)
            query = query.Where(x => x.ApprovedByUserId == filter.ApprovedByUserId.Value);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }
}