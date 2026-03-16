namespace CamE0.Storage.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(string category, string fileName, byte[] data);
    Task<byte[]?> ReadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<IReadOnlyList<StorageFileInfo>> ListFilesAsync(string category, DateTime? from = null, DateTime? to = null);
    Task<StorageStats> GetStorageStatsAsync();
    string GetStoragePath(string category);
}

public record StorageFileInfo(
    string FilePath,
    string FileName,
    string Category,
    long SizeBytes,
    DateTime CreatedAt
);

public record StorageStats(
    long TotalSpaceBytes,
    long UsedSpaceBytes,
    long AvailableSpaceBytes,
    int TotalFiles
);
