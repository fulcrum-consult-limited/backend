namespace Identity.Infrastructure.Services;

public sealed class MigrationService(AppDbContext context) : IMigrationService
{
    public async Task MigrateAsync(CancellationToken ct = default) =>
        await context.MigrateAsync(ct);
}