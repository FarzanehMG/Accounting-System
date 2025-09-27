using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Application.Repositories.JournalRepositories;
using Domain.Entities.Journal;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Journal;

internal class JournalVoucherRepository(AccountingDbContext dbContext) : BaseRepository<JournalVoucherEntity>(dbContext), IJournalVoucherRepository
{
    public Task CreateAsync(JournalVoucherEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JournalVoucherEntity?> GetLastJournalVoucherAsync(Guid financialPeriodId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JournalVoucherEntity?> GetByIdAsync(Guid journalVoucherId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JournalVoucherEntity?> GetByIdWithEntriesAsync(Guid journalVoucherId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<JournalVoucherEntity>> GetAllAsync(JournalVoucherFilter filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}