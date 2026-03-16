var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<LogoutHandler>();
builder.Services.AddScoped<CreateInvitationHandler>();
builder.Services.AddScoped<AcceptInvitationHandler>();
builder.Services.AddScoped<ResendInvitationHandler>();
builder.Services.AddScoped<GetInvitationByTokenHandler>();
builder.Services.AddScoped<ListPendingInvitationsHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();
builder.Services.AddScoped<ListUsersHandler>();
builder.Services.AddScoped<DeactivateUserHandler>();
builder.Services.AddScoped<ReactivateUserHandler>();
builder.Services.AddScoped<UpdateUserRoleHandler>();
builder.Services.AddScoped<RequestPasswordResetHandler>();
builder.Services.AddScoped<ResetPasswordHandler>();
builder.Services.AddScoped<ChangePasswordHandler>();
builder.Services.AddScoped<BootstrapHandler>();

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
            ClockSkew = TimeSpan.Zero
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

app.UseAuthentication();

app.UseMiddleware<TokenRevocationMiddleware>();

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
    await migrationService.MigrateAsync();
}

app.Run();