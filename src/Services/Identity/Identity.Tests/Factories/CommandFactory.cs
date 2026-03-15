using Bogus;
using Identity.Application.Auth.Commands.Login;
using Identity.Application.Auth.Commands.Logout;
using Identity.Application.Invitations.Commands.AcceptInvitation;
using Identity.Application.Invitations.Commands.CreateInvitation;
using Identity.Application.Invitations.Commands.ResendInvitation;
using Identity.Application.PasswordReset.Commands.ChangePassword;
using Identity.Application.PasswordReset.Commands.RequestPasswordReset;
using Identity.Application.PasswordReset.Commands.ResetPassword;
using Identity.Application.Setup.Commands.Bootstrap;
using Identity.Application.Users.Commands.DeactivateUser;
using Identity.Application.Users.Commands.ReactivateUser;
using Identity.Application.Users.Commands.UpdateUserRole;

namespace Identity.Tests.Factories;

public static class CommandFactory
{
    private static readonly Faker Faker = new("en") { Random = new Randomizer(1234) };

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