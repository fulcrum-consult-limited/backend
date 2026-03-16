namespace Identity.Tests.Domain;

[TestFixture]
public sealed class InvitationTests
{
    [Test]
    public void Create_GeneratesUniqueTokens()
    {
        var invitation1 = InvitationFactory.CreateValid();
        var invitation2 = InvitationFactory.CreateValid();

        Assert.That(invitation1.Token, Is.Not.EqualTo(invitation2.Token));
    }

    [Test]
    public void Create_DefaultsToNotUsed()
    {
        var invitation = InvitationFactory.CreateValid();

        Assert.That(invitation.IsUsed, Is.False);
    }

    [Test]
    public void Create_WithCustomExpiry_SetsCorrectExpiry()
    {
        var expiry = TimeSpan.FromHours(1);
        var before = DateTime.UtcNow;

        var invitation = InvitationFactory.CreateValid(expiry: expiry);

        Assert.That(invitation.ExpiresAt,
            Is.EqualTo(before.Add(expiry)).Within(TimeSpan.FromSeconds(1)));
    }

    // --- Use ---

    [Test]
    public void Use_ValidInvitation_MarksAsUsed()
    {
        var invitation = InvitationFactory.CreateValid();

        invitation.Use();

        Assert.Multiple(() =>
        {
            Assert.That(invitation.IsUsed, Is.True);
            Assert.That(invitation.UsedAt, Is.Not.Null);
        });
    }

    [Test]
    public void Use_AlreadyUsedInvitation_ThrowsInvitationAlreadyUsedException()
    {
        var invitation = InvitationFactory.CreateUsed();

        Assert.Throws<InvitationAlreadyUsedException>(() => invitation.Use());
    }

    [Test]
    public void Use_ExpiredInvitation_ThrowsInvitationExpiredException()
    {
        var invitation = InvitationFactory.CreateExpired();

        Assert.Throws<InvitationExpiredException>(() => invitation.Use());
    }

    [Test]
    public void Invalidate_SetsExpiryToNow()
    {
        var invitation = InvitationFactory.CreateInvalidated();
        
        Assert.That(invitation.IsExpired(), Is.True);
    }

    [Test]
    public void IsExpired_WhenNotExpired_ReturnsFalse()
    {
        var invitation = InvitationFactory.CreateValid();

        Assert.That(invitation.IsExpired(), Is.False);
    }

    [Test]
    public void IsExpired_WhenExpired_ReturnsTrue()
    {
        var invitation = InvitationFactory.CreateExpired();

        Assert.That(invitation.IsExpired(), Is.True);
    }
}