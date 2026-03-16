using CamE0.Security.Interfaces;
using CamE0.Security.Services;

namespace CamE0.Tests.Security;

public class EncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService = new AesEncryptionService();

    [Fact]
    public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalData()
    {
        var key = _encryptionService.GenerateKey();
        var plainData = System.Text.Encoding.UTF8.GetBytes("Hello, CamE0 Security!");

        var encrypted = _encryptionService.Encrypt(plainData, key);
        var decrypted = _encryptionService.Decrypt(encrypted, key);

        Assert.Equal(plainData, decrypted);
    }

    [Fact]
    public void EncryptString_DecryptString_RoundTrip_ReturnsOriginalString()
    {
        var passphrase = "test-passphrase-123";
        var original = "Sensitive camera data";

        var encrypted = _encryptionService.EncryptString(original, passphrase);
        var decrypted = _encryptionService.DecryptString(encrypted, passphrase);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var key = _encryptionService.GenerateKey();
        var plainData = System.Text.Encoding.UTF8.GetBytes("Same data");

        var encrypted1 = _encryptionService.Encrypt(plainData, key);
        var encrypted2 = _encryptionService.Encrypt(plainData, key);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void GenerateKey_ReturnsCorrectLength()
    {
        var key = _encryptionService.GenerateKey(32);
        Assert.Equal(32, key.Length);
    }

    [Fact]
    public void GenerateIv_ReturnsCorrectLength()
    {
        var iv = _encryptionService.GenerateIv(16);
        Assert.Equal(16, iv.Length);
    }

    [Fact]
    public void Encrypt_ThrowsOnNullData()
    {
        var key = _encryptionService.GenerateKey();
        Assert.Throws<ArgumentNullException>(() => _encryptionService.Encrypt(null!, key));
    }

    [Fact]
    public void Decrypt_ThrowsOnNullData()
    {
        var key = _encryptionService.GenerateKey();
        Assert.Throws<ArgumentNullException>(() => _encryptionService.Decrypt(null!, key));
    }
}
