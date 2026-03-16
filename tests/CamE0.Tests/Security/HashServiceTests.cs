using CamE0.Security.Interfaces;
using CamE0.Security.Services;

namespace CamE0.Tests.Security;

public class HashServiceTests
{
    private readonly IHashService _hashService = new HashService();

    [Fact]
    public void HashPassword_VerifyPassword_ReturnsTrue()
    {
        var password = "SecureP@ssw0rd";
        var hash = _hashService.HashPassword(password);

        Assert.True(_hashService.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _hashService.HashPassword("correct");
        Assert.False(_hashService.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void HashPassword_ProducesDifferentHashesForSamePassword()
    {
        var password = "test123";
        var hash1 = _hashService.HashPassword(password);
        var hash2 = _hashService.HashPassword(password);

        Assert.NotEqual(hash1, hash2); // BCrypt uses random salt
    }

    [Fact]
    public void ComputeSha256_ReturnsConsistentHash()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("test data");
        var hash1 = _hashService.ComputeSha256(data);
        var hash2 = _hashService.ComputeSha256(data);

        Assert.Equal(hash1, hash2);
        Assert.Equal(32, hash1.Length);
    }

    [Fact]
    public void ComputeSha256Hex_ReturnsHexString()
    {
        var hex = _hashService.ComputeSha256Hex("test");
        Assert.Equal(64, hex.Length);
        Assert.Matches("^[a-f0-9]+$", hex);
    }
}
