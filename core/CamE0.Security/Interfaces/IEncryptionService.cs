namespace CamE0.Security.Interfaces;

/// <summary>
/// Provides encryption and decryption services for data at rest and in transit.
/// </summary>
public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainData, byte[] key);
    byte[] Decrypt(byte[] cipherData, byte[] key);
    byte[] GenerateKey(int keySizeBytes = 32);
    byte[] GenerateIv(int ivSizeBytes = 16);
    string EncryptString(string plainText, string passphrase);
    string DecryptString(string cipherText, string passphrase);
}
