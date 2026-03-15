namespace Identity.Tests.Factories;

public static class InvitationFactory
{
    private static readonly Faker Faker = new("en") { Random = new Randomizer(1234) };

    public static Invitation CreateValid(
        Guid? userId = null,
        string? email = null,
        TimeSpan? expiry = null) =>
        Invitation.Create(
            userId ?? Faker.Random.Guid(),
            Email.Create(email ?? Faker.Internet.Email()),
            expiry ?? TimeSpan.FromHours(48));

    public static Invitation CreateExpired(
        Guid? userId = null,
        string? email = null) =>
        Invitation.Create(
            userId ?? Faker.Random.Guid(),
            Email.Create(email ?? Faker.Internet.Email()),
            TimeSpan.FromMilliseconds(-1));

    public static Invitation CreateUsed(
        Guid? userId = null,
        string? email = null)
    {
        var invitation = CreateValid(userId, email);
        invitation.Use();
        return invitation;
    }

    public static Invitation CreateInvalidated(
        Guid? userId = null,
        string? email = null)
    {
        var invitation = CreateValid(userId, email);
        invitation.Invalidate();
        return invitation;
    }
}