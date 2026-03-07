using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Abstractions.Persistence;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
