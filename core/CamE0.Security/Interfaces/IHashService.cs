namespace CamE0.Security.Interfaces;

/// <summary>
/// Provides hashing services for passwords and data integrity.
/// </summary>
public interface IHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    byte[] ComputeSha256(byte[] data);
    string ComputeSha256Hex(string input);
}
