using Domain.Entities;

namespace Application.Repositories.FinancialPeriod;

public interface IFinancialPeriodRepository
{
	Task CreateAsync(FinancialPeriodEntity financialPeriodEntity, CancellationToken cancellationToken = default);
    Task<FinancialPeriodEntity?> GetOverlappingFinancialPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task<FinancialPeriodEntity?> GetActiveFinancialPeriodAsync(CancellationToken cancellationToken);
}