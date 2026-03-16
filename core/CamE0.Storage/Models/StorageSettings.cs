namespace CamE0.Storage.Models;

public sealed class StorageSettings
{
    public string BasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CamE0", "storage");
    public long MaxStorageBytes { get; set; } = 100L * 1024 * 1024 * 1024; // 100 GB default
    public int RetentionDays { get; set; } = 30;
    public bool EncryptRecordings { get; set; }
    public string EncryptionPassphrase { get; set; } = string.Empty;
    public int SegmentDurationSeconds { get; set; } = 300; // 5 minute segments
}
