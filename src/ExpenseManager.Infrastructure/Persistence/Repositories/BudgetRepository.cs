using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence.Repositories;

public sealed class BudgetRepository(ExpenseManagerDbContext db) : IBudgetRepository
{
    public async Task<Budget?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default)
        => await db.Budgets
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Budget>> ListAsync(string userId, CancellationToken cancellationToken = default)
        => await db.Budgets
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
        => db.Budgets.AddAsync(budget, cancellationToken).AsTask();

    public void Remove(Budget budget) => db.Budgets.Remove(budget);
}
