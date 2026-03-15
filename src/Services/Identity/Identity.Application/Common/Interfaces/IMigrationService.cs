namespace Identity.Application.Common.Interfaces;

public interface IMigrationService
{
    Task MigrateAsync(CancellationToken ct = default);
}