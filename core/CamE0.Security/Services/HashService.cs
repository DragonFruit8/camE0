using System.Security.Cryptography;
using System.Text;
using CamE0.Security.Interfaces;

namespace CamE0.Security.Services;

/// <summary>
/// Provides BCrypt password hashing and SHA-256 data hashing.
/// </summary>
public sealed class HashService : IHashService
{
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public byte[] ComputeSha256(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return SHA256.HashData(data);
    }

    public string ComputeSha256Hex(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
