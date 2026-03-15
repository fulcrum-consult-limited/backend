namespace Identity.Tests.Domain;

[TestFixture]
public sealed class UserTests
{
    // --- CreatePending ---

    [Test]
    public void CreatePending_WithValidEmail_CreatesInactiveUser()
    {
        var user = UserFactory.CreatePending();

        Assert.Multiple(() =>
        {
            Assert.That(user.IsActive, Is.False);
            Assert.That(user.Role, Is.EqualTo(UserRole.User));
            Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public void CreatePending_RaisesInvitationCreatedDomainEvent()
    {
        var user = UserFactory.CreatePending();

        Assert.That(user.DomainEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void CreatePending_WithAdminRole_SetsRoleCorrectly()
    {
        var user = UserFactory.CreatePending(role: UserRole.Admin);

        Assert.That(user.Role, Is.EqualTo(UserRole.Admin));
    }


    [Test]
    public void CreateFirstAdmin_CreatesActiveAdminUser()
    {
        var user = UserFactory.CreateAdmin(forename: "Joe", surname: "Bloggs");

        Assert.Multiple(() =>
        {
            Assert.That(user.IsActive, Is.True);
            Assert.That(user.Role, Is.EqualTo(UserRole.Admin));
            Assert.That(user.Forename, Is.EqualTo("Joe"));
            Assert.That(user.Surname, Is.EqualTo("Bloggs"));
            Assert.That(user.FullName, Is.EqualTo("Joe Bloggs"));
        });
    }

    [Test]
    public void CreateFirstAdmin_WithBlankForename_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            UserFactory.CreateAdmin(forename: ""));
    }

    [Test]
    public void CreateFirstAdmin_WithBlankSurname_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            UserFactory.CreateAdmin(surname: ""));
    }

    // --- CompleteRegistration ---

    [Test]
    public void CompleteRegistration_WithValidData_ActivatesUser()
    {
        var user = UserFactory.CreateActive(forename: "Joe", surname: "Bloggs");

        Assert.Multiple(() =>
        {
            Assert.That(user.IsActive, Is.True);
            Assert.That(user.Forename, Is.EqualTo("Joe"));
            Assert.That(user.Surname, Is.EqualTo("Bloggs"));
        });
    }

    [Test]
    public void CompleteRegistration_WhenAlreadyActive_Throws()
    {
        var user = UserFactory.CreateAdmin();

        Assert.Throws<InvalidOperationException>(() =>
            user.CompleteRegistration("Joe", "Bloggs", "newhash"));
    }

    [Test]
    public void CompleteRegistration_RaisesUserRegisteredDomainEvent()
    {
        var user = UserFactory.CreatePending();
        user.ClearDomainEvents();

        user.CompleteRegistration("Joe", "Bloggs", "hash");

        Assert.That(user.DomainEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void CompleteRegistration_TrimsWhitespaceFromNames()
    {
        var user = UserFactory.CreatePending();

        user.CompleteRegistration("Joe", "Bloggs", "hash");

        Assert.Multiple(() =>
        {
            Assert.That(user.Forename, Is.EqualTo("Joe"));
            Assert.That(user.Surname, Is.EqualTo("Bloggs"));
        });
    }

    // --- Deactivate / Reactivate ---

    [Test]
    public void Deactivate_ActiveUser_SetsIsActiveToFalse()
    {
        var user = UserFactory.CreateAdmin();

        user.Deactivate();

        Assert.That(user.IsActive, Is.False);
    }

    [Test]
    public void Reactivate_InactiveUser_SetsIsActiveToTrue()
    {
        var user = UserFactory.CreateInactive();

        user.Reactivate();

        Assert.That(user.IsActive, Is.True);
    }

    // --- UpdateRole ---

    [Test]
    public void UpdateRole_ChangesRoleCorrectly()
    {
        var user = UserFactory.CreateAdmin();

        user.UpdateRole(UserRole.User);

        Assert.That(user.Role, Is.EqualTo(UserRole.User));
    }

    // --- ChangePassword ---

    [Test]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = UserFactory.CreateAdmin(passwordHash: "oldhash");

        user.ChangePassword("newhash");

        Assert.That(user.PasswordHash, Is.EqualTo("newhash"));
    }

    [Test]
    public void ChangePassword_WithBlankHash_Throws()
    {
        var user = UserFactory.CreateAdmin();

        Assert.Throws<ArgumentException>(() => user.ChangePassword(""));
    }
}