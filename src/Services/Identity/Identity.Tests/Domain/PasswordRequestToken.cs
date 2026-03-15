namespace Identity.Tests.Domain;

[TestFixture]
public sealed class PasswordResetTokenTests
{
    [Test]
    public void Create_GeneratesUniqueTokens()
    {
        var token1 = PasswordResetTokenFactory.CreateValid();
        var token2 = PasswordResetTokenFactory.CreateValid();

        Assert.That(token1.Token, Is.Not.EqualTo(token2.Token));
    }

    [Test]
    public void Create_DefaultExpiry_Is15Minutes()
    {
        var before = DateTime.UtcNow;

        var token = PasswordResetTokenFactory.CreateValid();

        Assert.That(token.ExpiresAt,
            Is.EqualTo(before.AddMinutes(15)).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Use_ValidToken_MarksAsUsed()
    {
        var token = PasswordResetTokenFactory.CreateValid();

        token.Use();

        Assert.Multiple(() =>
        {
            Assert.That(token.IsUsed, Is.True);
            Assert.That(token.UsedAt, Is.Not.Null);
        });
    }

    [Test]
    public void Use_AlreadyUsedToken_ThrowsPasswordResetTokenAlreadyUsedException()
    {
        var token = PasswordResetTokenFactory.CreateUsed();

        Assert.Throws<PasswordResetTokenAlreadyUsedException>(() => token.Use());
    }

    [Test]
    public void Use_ExpiredToken_ThrowsPasswordResetTokenExpiredException()
    {
        var token = PasswordResetTokenFactory.CreateExpired();

        Assert.Throws<PasswordResetTokenExpiredException>(() => token.Use());
    }

    [Test]
    public void IsExpired_WhenNotExpired_ReturnsFalse()
    {
        var token = PasswordResetTokenFactory.CreateValid();

        Assert.That(token.IsExpired(), Is.False);
    }

    [Test]
    public void IsExpired_WhenExpired_ReturnsTrue()
    {
        var token = PasswordResetTokenFactory.CreateExpired();

        Assert.That(token.IsExpired(), Is.True);
    }
}