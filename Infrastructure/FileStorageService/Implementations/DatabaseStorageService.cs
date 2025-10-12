using Application.Contracts.FileService.Interfaces;
using Application.Contracts.FileService.Models;
using Application.Repositories.Common;
using Domain.Entities.Attachment;
using FileTypeChecker;

namespace Infrastructure.FileStorageService.Implementations;

public class DatabaseStorageService(IUnitOfWork unitOfWork) : IFileService
{
    public async Task<List<SaveFileModelResult>> SaveFilesAsync(List<SaveFileModel> files, Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        var result = new List<SaveFileModelResult>();

        foreach (var file in files)
        {
            // Convert base64 string to bytes safely
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(file.Base64File);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid Base64 file data provided.");
            }

            // Try to detect file type via FileTypeChecker
            var extension = "bin";
            var mimeType = "application/octet-stream";
            var fileType = FileTypeValidator.GetFileType(fileBytes);
            if (fileType != null)
            {
                extension = fileType.Extension;
                mimeType = fileType.MimeType;
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid():N}.{extension}";

            // Store file in DB
            var attachment = new AttachmentEntity
            {
                FileName = fileName,
                ContentType = mimeType,
                FileData = fileBytes,
                JournalEntryId = journalEntryId
            };

            await unitOfWork.AttachmentRepository.CreateAsync(attachment, cancellationToken);

            // Prepare result model
            result.Add(new SaveFileModelResult
            {
                FileName = fileName,
                FileType = mimeType
            });
        }

        return result;
    }

    public async Task<List<GetFileModel>> GetFilesByNameAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        var attachments = await unitOfWork.AttachmentRepository.GetsByNameAsync(fileNames, cancellationToken);

        var result = attachments.Select(x => new GetFileModel
        {
            FileName = x.FileName,
            FileType = x.ContentType,
            Base64Data = Convert.ToBase64String(x.FileData!)
        }).ToList();

        return result;
    }

    public async Task RemoveFilesAsync(List<string> fileNames, CancellationToken cancellationToken = default)
    {
        var attachments = await unitOfWork.AttachmentRepository.GetsByNameAsync(fileNames, cancellationToken);
        if (!attachments.Any()) return;

        foreach (var attachment in attachments)
        {
            await unitOfWork.AttachmentRepository.RemoveAsync(attachment.FileName, cancellationToken);
        }
    }
}