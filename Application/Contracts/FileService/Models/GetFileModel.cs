namespace Application.Contracts.FileService.Models;

public record GetFileModel
{
    public required string FileType { get; init; }
    public required string FileName { get; init; }
    public required string Base64Data { get; init; }
}