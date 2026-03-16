namespace Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);

    Task<(IReadOnlyList<User> Users, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        UserRole? role = null,
        bool? isActive = null,
        string? searchTerm = null,
        CancellationToken ct = default);
}