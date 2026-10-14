namespace JobFlow.Application.Interfaces;

public interface IFileService
{
    Task<(string fileName, string fileUrl)> SaveFileAsync(Stream fileStream, string originalFileName, string folder, long maxSizeBytes, IEnumerable<string> allowedExtensions);
    void DeleteFile(string filePath);
    bool FileExists(string filePath);
}
