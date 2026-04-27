using Application.Repositories.Attachment;
using Domain.Entities.Attachment;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Attachment;

internal sealed class AttachmentRepository(AccountingDbContext dbContext) : BaseRepository<AttachmentEntity>(dbContext), IAttachmentRepository
{
    private readonly AccountingDbContext _dbContext = dbContext;

    public async Task<List<AttachmentEntity>> GetsByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        if (!fileNames.Any())
            return [];

        return await Table
            .Where(x => fileNames.Contains(x.FileName))
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(List<AttachmentEntity> entities, CancellationToken ct)
    {
        await _dbContext.Set<AttachmentEntity>().AddRangeAsync(entities, ct);
    }

    public async Task RemoveByNamesAsync(List<string> fileNames, CancellationToken ct)
    {
        if (fileNames.Count == 0)
            return;

        await _dbContext.Set<AttachmentEntity>()
            .Where(x => fileNames.Contains(x.FileName))
            .ExecuteDeleteAsync(ct);
    }
}