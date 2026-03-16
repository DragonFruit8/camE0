using CamE0.Security.Interfaces;
using CamE0.Security.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CamE0.Tests.Security;

public class AuditLoggerTests
{
    private readonly IAuditLogger _auditLogger;

    public AuditLoggerTests()
    {
        var loggerMock = new Mock<ILogger<InMemoryAuditLogger>>();
        _auditLogger = new InMemoryAuditLogger(loggerMock.Object);
    }

    [Fact]
    public async Task LogAsync_StoresEntry()
    {
        var entry = new AuditEntry(DateTime.UtcNow, "Test", "TestAction", "user1", "Test details");
        await _auditLogger.LogAsync(entry);

        var entries = await _auditLogger.GetEntriesAsync(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        Assert.Single(entries);
        Assert.Equal("TestAction", entries[0].Action);
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByCategory()
    {
        await _auditLogger.LogAsync(new AuditEntry(DateTime.UtcNow, "Auth", "Login", "user1", "Login"));
        await _auditLogger.LogAsync(new AuditEntry(DateTime.UtcNow, "System", "Start", "system", "Start"));

        var authEntries = await _auditLogger.GetEntriesAsync(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1), "Auth");
        Assert.Single(authEntries);
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByDateRange()
    {
        await _auditLogger.LogAsync(new AuditEntry(DateTime.UtcNow, "Test", "Action", "user1", "Test"));

        var futureEntries = await _auditLogger.GetEntriesAsync(
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));
        Assert.Empty(futureEntries);
    }
}
