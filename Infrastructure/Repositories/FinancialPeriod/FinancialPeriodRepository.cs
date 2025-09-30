using Application.Features.FinancialPeriod.Queries.Dtos;
using Application.Repositories.FinancialPeriod;
using Domain.Entities.FinancialPeriod;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.FinancialPeriod;

internal sealed class FinancialPeriodRepository(AccountingDbContext dbContext) : BaseRepository<FinancialPeriodEntity>(dbContext), IFinancialPeriodRepository
{
    public async Task CreateAsync(FinancialPeriodEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(CancellationToken cancellationToken = default, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x =>
                (!startDate.HasValue || x.EndDate >= startDate.Value) &&
                (!endDate.HasValue || x.StartDate <= endDate.Value),
            cancellationToken);

    }

    public async Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
    }

    public async Task<FinancialPeriodEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialPeriodEntity>> GetAllAsync(FinancialPeriodFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking;

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(x => x.FinancialPeriodTitle.Equals(filter.Title));

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(x => x.StartDate == filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(x => x.EndDate == filter.EndDate.Value);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }
}