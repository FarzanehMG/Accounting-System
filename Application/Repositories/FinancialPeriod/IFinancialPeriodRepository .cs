using Application.Features.FinancialPeriod.Queries.Dtos;
using Domain.Entities.FinancialPeriod;

namespace Application.Repositories.FinancialPeriod;

public interface IFinancialPeriodRepository
{
	Task CreateAsync(FinancialPeriodEntity entity, CancellationToken cancellationToken = default);
    Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(CancellationToken cancellationToken = default, DateTime? startDate = null, DateTime? endDate = null);
    Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken = default);
    Task<FinancialPeriodEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FinancialPeriodEntity>> GetAllAsync(FinancialPeriodFilter filter, CancellationToken cancellationToken = default);
}