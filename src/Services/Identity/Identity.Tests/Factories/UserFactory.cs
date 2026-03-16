namespace Identity.Tests.Factories;

public static class UserFactory
{
    private static readonly Faker Faker = new() { Random = new Randomizer(1234) };

    public static User CreateAdmin(
        string? email = null,
        string? forename = null,
        string? surname = null,
        string passwordHash = "hashedpassword") =>
        User.CreateFirstAdmin(
            Email.Create(email ?? Faker.Internet.Email()),
            forename ?? Faker.Name.FirstName(),
            surname ?? Faker.Name.LastName(),
            passwordHash);

    public static User CreatePending(
        string? email = null,
        UserRole role = UserRole.User) =>
        User.CreatePending(
            Email.Create(email ?? Faker.Internet.Email()),
            role);

    public static User CreateActive(
        string? email = null,
        string? forename = null,
        string? surname = null,
        string passwordHash = "hashedpassword")
    {
        var user = User.CreatePending(
            Email.Create(email ?? Faker.Internet.Email()));

        user.CompleteRegistration(
            forename ?? Faker.Name.FirstName(),
            surname ?? Faker.Name.LastName(),
            passwordHash);

        return user;
    }

    public static User CreateInactive(
        string? email = null,
        string? forename = null,
        string? surname = null)
    {
        var user = CreateAdmin(email, forename, surname);
        user.Deactivate();
        return user;
    }
}