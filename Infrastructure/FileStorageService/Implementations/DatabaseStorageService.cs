using Application.Contracts.FileService.Interfaces;
using Application.Contracts.FileService.Models;
using Application.Repositories.Common;
using Domain.Entities.Attachment;
using FileTypeChecker;

namespace Infrastructure.FileStorageService.Implementations;

public class DatabaseStorageService(IUnitOfWork unitOfWork) : IFileService
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public async Task<List<SaveFileModelResult>> SaveFilesAsync(List<SaveFileModel> files, Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
            return [];

        var attachments = new List<AttachmentEntity>();
        var result = new List<SaveFileModelResult>();

        foreach (var file in files)
        {
            // Convert base64 string to bytes safely
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(file.Base64File);
            }
            catch
            {
                throw new InvalidOperationException("Invalid Base64 file data provided.");
            }

            if (fileBytes.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("File size exceeds allowed limit (5MB).");

            // Try to detect file type via FileTypeChecker
            var fileType = FileTypeValidator.GetFileType(fileBytes);

            var extension = fileType?.Extension ?? "bin";
            var mimeType = fileType?.MimeType ?? "application/octet-stream";

            // Generate unique file name
            var fileName = $"{Guid.NewGuid():N}.{extension}";

            // Store file in DB
            attachments.Add(new AttachmentEntity
            {
                FileName = fileName,
                ContentType = mimeType,
                FileData = fileBytes,
                JournalEntryId = journalEntryId
            });

            // Prepare result model
            result.Add(new SaveFileModelResult
            {
                FileName = fileName,
                FileType = mimeType
            });
        }

        // Single batch insert (important for performance + consistency)
        await unitOfWork.AttachmentRepository.AddRangeAsync(attachments, cancellationToken);

        return result;
    }

    public async Task<List<GetFileModel>> GetFilesByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        if (fileNames.Count == 0)
            return [];

        var attachments = await unitOfWork.AttachmentRepository
            .GetsByNameAsync(fileNames, cancellationToken);

        return attachments.Select(x => new GetFileModel
        {
            FileName = x.FileName,
            FileType = x.ContentType,
            Base64Data = Convert.ToBase64String(x.FileData!)
        }).ToList();
    }

    public async Task RemoveFilesAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        if (fileNames.Count == 0)
            return;

        // Bulk delete instead of per-row delete
        await unitOfWork.AttachmentRepository
            .RemoveByNamesAsync(fileNames, cancellationToken);
    }
}