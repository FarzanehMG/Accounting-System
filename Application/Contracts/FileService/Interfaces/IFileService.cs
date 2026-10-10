using Application.Contracts.FileService.Models;

namespace Application.Contracts.FileService.Interfaces;

public interface IFileService
{
    Task<List<SaveFileModelResult>> SaveFilesAsync(List<SaveFileModel> files, Guid journalEntryId, CancellationToken cancellationToken = default);
    Task<List<GetFileModel>> GetFilesByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default);
    Task RemoveFilesAsync(List<string> fileNames, CancellationToken cancellationToken = default);
}