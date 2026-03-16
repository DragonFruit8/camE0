using System.Collections.Concurrent;
using CamE0.Security.Interfaces;
using Microsoft.Extensions.Logging;

namespace CamE0.Security.Services;

/// <summary>
/// In-memory audit logger with optional file persistence.
/// </summary>
public sealed class InMemoryAuditLogger : IAuditLogger
{
    private readonly ConcurrentBag<AuditEntry> _entries = new();
    private readonly ILogger<InMemoryAuditLogger> _logger;

    public InMemoryAuditLogger(ILogger<InMemoryAuditLogger> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
        _logger.LogInformation(
            "AUDIT: [{Category}] {Action} by {UserId} - {Details}",
            entry.Category, entry.Action, entry.UserId, entry.Details);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEntry>> GetEntriesAsync(DateTime from, DateTime to, string? category = null)
    {
        var query = _entries.Where(e => e.Timestamp >= from && e.Timestamp <= to);
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
        IReadOnlyList<AuditEntry> result = query.OrderByDescending(e => e.Timestamp).ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
