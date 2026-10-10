using Domain.Entities.Attachment;

namespace Application.Repositories.Attachment;

public interface IAttachmentRepository
{
    Task<List<AttachmentEntity>> GetsByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<AttachmentEntity> entities, CancellationToken cancellationToken = default);
    Task RemoveByNamesAsync(List<string> fileNames, CancellationToken cancellationToken = default);
}