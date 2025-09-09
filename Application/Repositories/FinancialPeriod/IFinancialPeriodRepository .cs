using Domain.Entities;

namespace Application.Repositories.FinancialPeriod;

public interface IFinancialPeriodRepository
{
	Task CreateAsync(FinancialPeriodEntity financialPeriodEntity, CancellationToken cancellationToken = default);
    Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(CancellationToken cancellationToken = default, DateTime? startDate = null, DateTime? endDate = null);
    Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken = default);
    Task<FinancialPeriodEntity?> GetFinancialPeriodByIdAsync(Guid financialPeriodId, CancellationToken cancellationToken = default);
}