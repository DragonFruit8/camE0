namespace CamE0.Security.Interfaces;

/// <summary>
/// Provides audit logging for security-relevant events.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(AuditEntry entry);
    Task<IReadOnlyList<AuditEntry>> GetEntriesAsync(DateTime from, DateTime to, string? category = null);
}

public record AuditEntry(
    DateTime Timestamp,
    string Category,
    string Action,
    string UserId,
    string Details,
    string? IpAddress = null
);
