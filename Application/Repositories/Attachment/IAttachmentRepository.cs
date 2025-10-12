using Domain.Entities.Attachment;

namespace Application.Repositories.Attachment;

public interface IAttachmentRepository
{
	Task CreateAsync(AttachmentEntity entity, CancellationToken cancellationToken = default);
    Task<List<AttachmentEntity>> GetsByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string fileName, CancellationToken cancellationToken = default);
}