namespace Identity.Application.Common.Errors;

public static class IdentityErrors
{
    public static readonly Error UserNotFound =
        new("identity.user.not_found", "User not found.");

    public static readonly Error UserAlreadyExists =
        new("identity.user.already_exists", "A user with this email already exists.");

    public static readonly Error UserInactive =
        new("identity.user.inactive", "This account is inactive.");

    public static readonly Error UserAlreadyRegistered =
        new("identity.user.already_registered", "This user has already completed registration.");

    public static readonly Error InvalidCredentials =
        new("identity.credentials.invalid", "Email or password is incorrect.");

    public static readonly Error WeakPassword =
        new("identity.credentials.weak_password", "Password does not meet complexity requirements.");

    public static readonly Error InvalidEmail =
        new("identity.credentials.invalid_email", "The provided email address is not valid.");

    public static readonly Error InvitationNotFound =
        new("identity.invitation.not_found", "Invitation not found.");

    public static readonly Error InvitationExpired =
        new("identity.invitation.expired", "This invitation link has expired.");

    public static readonly Error InvitationAlreadyUsed =
        new("identity.invitation.already_used", "This invitation link has already been used.");

    public static readonly Error PendingInvitationExists =
        new("identity.invitation.pending_exists", "An active invitation already exists for this user.");

    public static readonly Error PasswordResetTokenNotFound =
        new("identity.password_reset.not_found", "Password reset token not found.");

    public static readonly Error PasswordResetTokenExpired =
        new("identity.password_reset.expired", "This password reset link has expired.");

    public static readonly Error PasswordResetTokenAlreadyUsed =
        new("identity.password_reset.already_used", "This password reset link has already been used.");

    public static readonly Error BootstrapAlreadyCompleted =
        new("identity.setup.already_completed",
            "Setup has already been completed. An admin user already exists.");
    public static readonly Error TokenRevocationFailed =
        new("identity.token.revocation_failed", "Failed to revoke the session token.");
}