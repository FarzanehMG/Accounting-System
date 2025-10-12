using Application.Repositories.Attachment;
using Domain.Entities.Attachment;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Attachment;

internal sealed class AttachmentRepository(AccountingDbContext dbContext) : BaseRepository<AttachmentEntity>(dbContext), IAttachmentRepository
{
    public async Task CreateAsync(AttachmentEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<List<AttachmentEntity>> GetsByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        if (!fileNames.Any())
            return [];

        return await Table
            .Where(x => fileNames.Contains(x.FileName))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var entity = await Table.FirstOrDefaultAsync(x => x.FileName == fileName, cancellationToken);
        if (entity == null)
            return false;

        Remove(entity);
        return true;
    }
}