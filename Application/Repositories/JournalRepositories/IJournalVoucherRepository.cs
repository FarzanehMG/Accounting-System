using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Domain.Entities;

namespace Application.Repositories.JournalRepositories;

public interface IJournalVoucherRepository
{
    Task CreateAsync(JournalVoucherEntity entity, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetLastJournalVoucherAsync(Guid financialPeriodId, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetByIdAsync(Guid journalVoucherId, CancellationToken cancellationToken = default);
    Task<JournalVoucherEntity?> GetByIdWithEntriesAsync(Guid journalVoucherId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalVoucherEntity>> GetAllAsync(JournalVoucherFilter filter, CancellationToken cancellationToken = default);
}