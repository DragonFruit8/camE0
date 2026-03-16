using CamE0.Security.Interfaces;
using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CamE0.Storage.Services;

public sealed class LocalStorageService : IStorageService
{
    private readonly StorageSettings _settings;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(
        IOptions<StorageSettings> settings,
        IEncryptionService encryptionService,
        ILogger<LocalStorageService> logger)
    {
        _settings = settings.Value;
        _encryptionService = encryptionService;
        _logger = logger;
        EnsureDirectoriesExist();
    }

    public async Task<string> SaveFileAsync(string category, string fileName, byte[] data)
    {
        var categoryPath = GetStoragePath(category);
        var datePath = Path.Combine(categoryPath, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(datePath);

        var filePath = Path.Combine(datePath, fileName);

        var dataToWrite = data;
        if (_settings.EncryptRecordings && !string.IsNullOrEmpty(_settings.EncryptionPassphrase))
        {
            var key = DeriveKey(_settings.EncryptionPassphrase);
            dataToWrite = _encryptionService.Encrypt(data, key);
            filePath += ".enc";
        }

        await File.WriteAllBytesAsync(filePath, dataToWrite);
        _logger.LogDebug("Saved file: {FilePath} ({Size} bytes)", filePath, dataToWrite.Length);

        return filePath;
    }

    public async Task<byte[]?> ReadFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var data = await File.ReadAllBytesAsync(filePath);

        if (filePath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)
            && _settings.EncryptRecordings
            && !string.IsNullOrEmpty(_settings.EncryptionPassphrase))
        {
            var key = DeriveKey(_settings.EncryptionPassphrase);
            data = _encryptionService.Decrypt(data, key);
        }

        return data;
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        _logger.LogInformation("Deleted file: {FilePath}", filePath);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<StorageFileInfo>> ListFilesAsync(string category, DateTime? from = null, DateTime? to = null)
    {
        var categoryPath = GetStoragePath(category);
        if (!Directory.Exists(categoryPath))
        {
            return Task.FromResult<IReadOnlyList<StorageFileInfo>>(Array.Empty<StorageFileInfo>());
        }

        var files = Directory.GetFiles(categoryPath, "*.*", SearchOption.AllDirectories)
            .Select(f => new FileInfo(f))
            .Where(f =>
            {
                if (from.HasValue && f.CreationTimeUtc < from.Value) return false;
                if (to.HasValue && f.CreationTimeUtc > to.Value) return false;
                return true;
            })
            .Select(f => new StorageFileInfo(f.FullName, f.Name, category, f.Length, f.CreationTimeUtc))
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<StorageFileInfo>>(files.AsReadOnly());
    }

    public Task<StorageStats> GetStorageStatsAsync()
    {
        var basePath = _settings.BasePath;
        Directory.CreateDirectory(basePath);

        var driveInfo = new DriveInfo(Path.GetPathRoot(basePath) ?? basePath);
        var totalFiles = Directory.Exists(basePath)
            ? Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories).Length
            : 0;
        var usedSpace = Directory.Exists(basePath)
            ? Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length)
            : 0L;

        return Task.FromResult(new StorageStats(
            driveInfo.TotalSize,
            usedSpace,
            driveInfo.AvailableFreeSpace,
            totalFiles));
    }

    public string GetStoragePath(string category)
    {
        return Path.Combine(_settings.BasePath, category);
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_settings.BasePath);
        Directory.CreateDirectory(GetStoragePath("recordings"));
        Directory.CreateDirectory(GetStoragePath("snapshots"));
        Directory.CreateDirectory(GetStoragePath("exports"));
    }

    private static byte[] DeriveKey(string passphrase)
    {
        var salt = System.Text.Encoding.UTF8.GetBytes("CamE0_Storage_Salt_v1");
        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            passphrase, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }
}
