namespace Identity.Tests.Factories;

public static class CommandFactory
{
    private static readonly Faker Faker = new() { Random = new Randomizer(1234) };

    public const string ValidPassword = "Test@1234";

    public static BootstrapCommand Bootstrap(
        string? email = null,
        string? forename = null,
        string? surname = null,
        string? password = null) => new(
        email ?? Faker.Internet.Email(),
        forename ?? Faker.Name.FirstName(),
        surname ?? Faker.Name.LastName(),
        password ?? ValidPassword);

    public static LoginCommand Login(
        string? email = null,
        string? password = null) => new(
        email ?? Faker.Internet.Email(),
        password ?? ValidPassword);

    public static LogoutCommand Logout(
        string? token = null) => new(
        token ?? Faker.Random.AlphaNumeric(64));

    public static CreateInvitationCommand CreateInvitation(
        string? email = null,
        UserRole role = UserRole.User) => new(
        email ?? Faker.Internet.Email(),
        role);

    public static AcceptInvitationCommand AcceptInvitation(
        string? token = null,
        string? forename = null,
        string? surname = null,
        string? password = null) => new(
        token ?? Faker.Random.AlphaNumeric(64),
        forename ?? Faker.Name.FirstName(),
        surname ?? Faker.Name.LastName(),
        password ?? ValidPassword);

    public static ResendInvitationCommand ResendInvitation(
        Guid? userId = null) => new(
        userId ?? Faker.Random.Guid());

    public static RequestPasswordResetCommand RequestPasswordReset(
        string? email = null) => new(
        email ?? Faker.Internet.Email());

    public static ResetPasswordCommand ResetPassword(
        string? token = null,
        string? newPassword = null) => new(
        token ?? Faker.Random.AlphaNumeric(64),
        newPassword ?? ValidPassword);

    public static ChangePasswordCommand ChangePassword(
        Guid? userId = null,
        string? currentPassword = null,
        string? newPassword = null) => new(
        userId ?? Faker.Random.Guid(),
        currentPassword ?? ValidPassword,
        newPassword ?? "NewTest@1234");

    public static DeactivateUserCommand DeactivateUser(
        Guid? userId = null) => new(
        userId ?? Faker.Random.Guid());

    public static ReactivateUserCommand ReactivateUser(
        Guid? userId = null) => new(
        userId ?? Faker.Random.Guid());

    public static UpdateUserRoleCommand UpdateUserRole(
        Guid? userId = null,
        UserRole role = UserRole.User) => new(
        userId ?? Faker.Random.Guid(),
        role);
}