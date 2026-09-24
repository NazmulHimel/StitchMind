namespace GarmentsERP.Shared.Interfaces;

public record FileUploadResult(
    bool Success,
    string FilePath,
    string PublicUrl,
    string FileName,
    long FileSizeBytes,
    string ContentType);

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string filePath, CancellationToken ct = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken ct = default);
    string GetPublicUrl(string filePath);
}
