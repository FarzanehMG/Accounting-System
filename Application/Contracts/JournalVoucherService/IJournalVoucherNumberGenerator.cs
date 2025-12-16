namespace Application.Contracts.JournalVoucherService;

public interface IJournalVoucherNumberGenerator
{
    Task<int> GenerateAsync(Guid financialPeriodId, CancellationToken cancellationToken);
}