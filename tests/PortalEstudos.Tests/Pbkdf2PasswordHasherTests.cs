using PortalEstudos.Infrastructure.Security;
using Xunit;

namespace PortalEstudos.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ProducesFormattedHash()
    {
        var hash = _hasher.HashPassword("SenhaSegura123");

        Assert.NotNull(hash);
        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.Equal("100000", parts[0]);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.HashPassword("Portal@123");

        var isValid = _hasher.VerifyPassword("Portal@123", hash);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.HashPassword("Portal@123");

        var isValid = _hasher.VerifyPassword("SenhaErrada", hash);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalido")]
    [InlineData("100.invalido")]
    [InlineData("abc.salt.hash")]
    public void VerifyPassword_WithCorruptedHash_ReturnsFalse(string badHash)
    {
        var isValid = _hasher.VerifyPassword("Portal@123", badHash);

        Assert.False(isValid);
    }
}
