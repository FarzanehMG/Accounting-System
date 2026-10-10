namespace Application.Contracts.FileService.Models;

public record SaveFileModel
{
    public required string Base64File { get; set; }
    public required string FileContent { get; set; }
}