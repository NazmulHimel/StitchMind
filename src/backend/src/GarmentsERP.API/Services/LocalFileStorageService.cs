using GarmentsERP.Shared.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GarmentsERP.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration config)
    {
        _baseStoragePath = config["FileStorage:LocalPath"] 
            ?? @"E:\Garments Project\GarmentsERP\storage\uploads";
        _baseUrl = config["FileStorage:BaseUrl"] ?? "/api/v1/files";

        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default)
    {
        try
        {
            var targetFolder = Path.Combine(_baseStoragePath, folder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(targetFolder, uniqueFileName);

            using (var destination = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(destination, ct);
            }

            var relativePath = Path.Combine(folder, uniqueFileName).Replace("\\", "/");
            var publicUrl = $"{_baseUrl}/{relativePath}";
            var fileInfo = new FileInfo(fullPath);

            return new FileUploadResult(
                Success: true,
                FilePath: relativePath,
                PublicUrl: publicUrl,
                FileName: fileName,
                FileSizeBytes: fileInfo.Length,
                ContentType: GetContentType(extension));
        }
        catch (Exception)
        {
            return new FileUploadResult(false, string.Empty, string.Empty, fileName, 0, string.Empty);
        }
    }

    public Task<Stream> DownloadAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found on storage.", filePath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public string GetPublicUrl(string filePath)
    {
        return $"{_baseUrl}/{filePath.Replace("\\", "/")}";
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".csv" => "text/csv",
        _ => "application/octet-stream"
    };
}
