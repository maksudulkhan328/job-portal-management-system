using Microsoft.Extensions.Configuration;
using JobFlow.Application.Interfaces;

namespace JobFlow.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _baseUploadPath;
    private readonly string _baseUrl;

    public FileService(IConfiguration config)
    {
        _baseUploadPath = config["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = config["FileStorage:BaseUrl"] ?? "/uploads";
    }

    public async Task<(string fileName, string fileUrl)> SaveFileAsync(
        Stream fileStream, string originalFileName, string folder,
        long maxSizeBytes, IEnumerable<string> allowedExtensions)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");

        if (fileStream.Length > maxSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxSizeBytes / 1024 / 1024}MB.");

        var uploadDir = Path.Combine(_baseUploadPath, folder);
        Directory.CreateDirectory(uploadDir);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadDir, uniqueFileName);

        using var fileStreamOut = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut);

        var fileUrl = $"{_baseUrl}/{folder}/{uniqueFileName}";
        return (uniqueFileName, fileUrl);
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public bool FileExists(string filePath) => File.Exists(filePath);
}
