using Application.Features.FinancialPeriod.Queries.Dtos;
using Application.Repositories.FinancialPeriod;
using Domain.Entities.FinancialPeriod;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.FinancialPeriod;

internal sealed class FinancialPeriodRepository(AccountingDbContext dbContext) : BaseRepository<FinancialPeriodEntity>(dbContext), IFinancialPeriodRepository
{
    private readonly AccountingDbContext _dbContext = dbContext;

    public async Task CreateAsync(FinancialPeriodEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(FinancialPeriodEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<FinancialPeriodEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public async Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(CancellationToken cancellationToken = default, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x =>
                (!startDate.HasValue || x.EndDate.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || x.StartDate.Date <= endDate.Value.Date),
                cancellationToken);

    }

    public async Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodByIdAsync(Guid id, CancellationToken cancellationToken = default, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x =>
                (!startDate.HasValue || x.EndDate.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || x.StartDate.Date <= endDate.Value.Date) &&
                x.Id != id,
            cancellationToken);
    }

    public async Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
    }

    public async Task<FinancialPeriodEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(x=>x.JournalVouchers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialPeriodEntity>> GetAllAsync(FinancialPeriodFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking;

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(x => x.FinancialPeriodTitle.Equals(filter.Title));

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(x => x.StartDate.Date == filter.StartDate.Value.Date);

        if (filter.EndDate.HasValue)
            query = query.Where(x => x.EndDate.Date == filter.EndDate.Value.Date);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }
}