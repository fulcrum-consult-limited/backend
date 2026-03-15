namespace Identity.Tests.Factories;

public static class PasswordResetTokenFactory
{
    private static readonly Faker Faker = new("en") { Random = new Randomizer(1234) };

    public static PasswordResetToken CreateValid(
        Guid? userId = null,
        string? email = null,
        TimeSpan? expiry = null) =>
        PasswordResetToken.Create(
            userId ?? Faker.Random.Guid(),
            Email.Create(email ?? Faker.Internet.Email()),
            expiry ?? TimeSpan.FromMinutes(15));

    public static PasswordResetToken CreateExpired(
        Guid? userId = null,
        string? email = null) =>
        PasswordResetToken.Create(
            userId ?? Faker.Random.Guid(),
            Email.Create(email ?? Faker.Internet.Email()),
            TimeSpan.FromMilliseconds(-1));

    public static PasswordResetToken CreateUsed(
        Guid? userId = null,
        string? email = null)
    {
        var token = CreateValid(userId, email);
        token.Use();
        return token;
    }
}