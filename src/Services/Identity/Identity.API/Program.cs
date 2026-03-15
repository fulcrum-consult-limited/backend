using System.Text;
using Identity.API.Middleware;
using Identity.Application.Common.Interfaces;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure — registers DB, repositories, JWT, BCrypt, Redis
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Handlers — registered as scoped since they depend on scoped repositories
builder.Services.AddScoped<Identity.Application.Auth.Commands.Login.LoginHandler>();
builder.Services.AddScoped<Identity.Application.Auth.Commands.Logout.LogoutHandler>();
builder.Services.AddScoped<Identity.Application.Invitations.Commands.CreateInvitation.CreateInvitationHandler>();
builder.Services.AddScoped<Identity.Application.Invitations.Commands.AcceptInvitation.AcceptInvitationHandler>();
builder.Services.AddScoped<Identity.Application.Invitations.Commands.ResendInvitation.ResendInvitationHandler>();
builder.Services.AddScoped<Identity.Application.Invitations.Queries.GetInvitationByToken.GetInvitationByTokenHandler>();
builder.Services.AddScoped<Identity.Application.Invitations.Queries.ListPendingInvitations.ListPendingInvitationsHandler>();
builder.Services.AddScoped<Identity.Application.Users.Queries.GetUserById.GetUserByIdHandler>();
builder.Services.AddScoped<Identity.Application.Users.Queries.GetUserByEmail.GetUserByEmailHandler>();
builder.Services.AddScoped<Identity.Application.Users.Queries.ListUsers.ListUsersHandler>();
builder.Services.AddScoped<Identity.Application.Users.Commands.DeactivateUser.DeactivateUserHandler>();
builder.Services.AddScoped<Identity.Application.Users.Commands.ReactivateUser.ReactivateUserHandler>();
builder.Services.AddScoped<Identity.Application.Users.Commands.UpdateUserRole.UpdateUserRoleHandler>();
builder.Services.AddScoped<Identity.Application.PasswordReset.Commands.RequestPasswordReset.RequestPasswordResetHandler>();
builder.Services.AddScoped<Identity.Application.PasswordReset.Commands.ResetPassword.ResetPasswordHandler>();
builder.Services.AddScoped<Identity.Application.PasswordReset.Commands.ChangePassword.ChangePasswordHandler>();
builder.Services.AddScoped<Identity.Application.Setup.Commands.Bootstrap.BootstrapHandler>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // No grace period — tokens expire exactly on time
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication must come before the revocation middleware
app.UseAuthentication();

// Revocation check runs after authentication so User claims are populated
app.UseMiddleware<TokenRevocationMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Auto-migrate on startup via IMigrationService — API never references EF Core directly
using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
    await migrationService.MigrateAsync();
}

app.Run();