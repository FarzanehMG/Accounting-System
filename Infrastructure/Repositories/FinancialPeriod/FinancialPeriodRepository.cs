using Application.Features.FinancialPeriod.Queries.Dtos;
using Application.Repositories.FinancialPeriod;
using Domain.Entities.FinancialPeriod;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.FinancialPeriod;

internal class FinancialPeriodRepository(AccountingDbContext dbContext) : BaseRepository<FinancialPeriodEntity>(dbContext), IFinancialPeriodRepository
{
    public Task CreateAsync(FinancialPeriodEntity financialPeriodEntity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(CancellationToken cancellationToken = default, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        throw new NotImplementedException();
    }

    public Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FinancialPeriodEntity?> GetFinancialPeriodByIdAsync(Guid financialPeriodId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FinancialPeriodEntity>> GetAllAsync(FinancialPeriodFilter financialPeriodFilter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}