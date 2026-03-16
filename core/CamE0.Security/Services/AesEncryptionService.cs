using System.Security.Cryptography;
using System.Text;
using CamE0.Security.Interfaces;

namespace CamE0.Security.Services;

/// <summary>
/// AES-256-CBC encryption service for data at rest.
/// </summary>
public sealed class AesEncryptionService : IEncryptionService
{
    public byte[] Encrypt(byte[] plainData, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(plainData);
        ArgumentNullException.ThrowIfNull(key);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);

        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

        return result;
    }

    public byte[] Decrypt(byte[] cipherData, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(cipherData);
        ArgumentNullException.ThrowIfNull(key);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV from beginning of ciphertext
        var iv = new byte[aes.BlockSize / 8];
        Buffer.BlockCopy(cipherData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        var cipherBytes = new byte[cipherData.Length - iv.Length];
        Buffer.BlockCopy(cipherData, iv.Length, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
    }

    public byte[] GenerateKey(int keySizeBytes = 32)
    {
        return RandomNumberGenerator.GetBytes(keySizeBytes);
    }

    public byte[] GenerateIv(int ivSizeBytes = 16)
    {
        return RandomNumberGenerator.GetBytes(ivSizeBytes);
    }

    public string EncryptString(string plainText, string passphrase)
    {
        var key = DeriveKey(passphrase);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = Encrypt(plainBytes, key);
        return Convert.ToBase64String(encrypted);
    }

    public string DecryptString(string cipherText, string passphrase)
    {
        var key = DeriveKey(passphrase);
        var cipherBytes = Convert.FromBase64String(cipherText);
        var decrypted = Decrypt(cipherBytes, key);
        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] DeriveKey(string passphrase)
    {
        var salt = Encoding.UTF8.GetBytes("CamE0_Security_Salt_v1");
        using var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }
}
