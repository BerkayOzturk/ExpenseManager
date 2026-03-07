using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence.Repositories;

public sealed class UserSettingsRepository(ExpenseManagerDbContext db) : IUserSettingsRepository
{
    public async Task<UserSettings?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default)
        => db.UserSettings.AddAsync(settings, cancellationToken).AsTask();
}
