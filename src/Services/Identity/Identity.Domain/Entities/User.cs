namespace Identity.Domain.Entities;

public sealed class User : BaseEntity
{
    public Email Email { get; private init; } = null!;
    public string Forename { get; private set; } = string.Empty;
    public string Surname { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{Forename} {Surname}";

    private User() { }

    public static User CreatePending(Email email, UserRole role = UserRole.User)
    {
        var user = new User
        {
            Email = email,
            Role = role,
            IsActive = false
        };

        user.RaiseDomainEvent(new InvitationCreatedDomainEvent(Guid.NewGuid(), DateTime.UtcNow, user.Id, email));

        return user;
    }

    public void CompleteRegistration(string forename, string surname, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(forename);
        ArgumentException.ThrowIfNullOrWhiteSpace(surname);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        if (IsActive)
            throw new InvalidOperationException("User is already registered.");

        Forename = forename.Trim();
        Surname = surname.Trim();
        PasswordHash = passwordHash;
        IsActive = true;
        SetUpdatedAt();

        RaiseDomainEvent(new UserRegisteredDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Id, Email));
    }

    public static User CreateFirstAdmin(
        Email email,
        string forename,
        string surname,
        string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(forename);
        ArgumentException.ThrowIfNullOrWhiteSpace(surname);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User
        {
            Email = email,
            Forename = forename.Trim(),
            Surname = surname.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsActive = true
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, user.Id, email));

        return user;
    }

    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);

        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void RaisePasswordResetRequestedEvent(Guid tokenId)
    {
        RaiseDomainEvent(new PasswordResetRequestedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Email));
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}

public enum UserRole
{
    User,
    Admin
}