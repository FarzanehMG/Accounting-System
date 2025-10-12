namespace Application.Contracts.FileService.Models;

public record SaveFileModelResult
{
    public required string FileName { get; init; }
    public required string FileType { get; init; }
}