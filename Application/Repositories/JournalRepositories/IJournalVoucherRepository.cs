using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Domain.Entities.Journal;
using Domain.Enums;

namespace Application.Repositories.JournalRepositories;

public interface IJournalVoucherRepository
{
    Task CreateAsync(JournalVoucherEntity entity, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetLastJournalVoucherAsync(Guid financialPeriodId, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetByIdAsync(Guid journalVoucherId, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetByIdWithEntriesAsync(Guid journalVoucherId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalVoucherEntity>> GetAllAsync(JournalVoucherFilter filter, CancellationToken cancellationToken = default);
    Task<bool> HasUnPostedTransactionsAsync(Guid financialPeriodId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalVoucherEntity>> GetByFinancialPeriodAndStateAsync(Guid financialPeriodId, JournalVoucherState state, CancellationToken cancellationToken);
    Task<JournalVoucherEntity> GetByFinancialPeriodAsync(Guid financialPeriodId, CancellationToken cancellationToken);
    Task UpdateAsync(JournalVoucherEntity entity, CancellationToken cancellationToken = default);

    public Task<int> GetMaxVoucherNumberAsync(Guid financialPeriodId, CancellationToken cancellationToken = default);
}