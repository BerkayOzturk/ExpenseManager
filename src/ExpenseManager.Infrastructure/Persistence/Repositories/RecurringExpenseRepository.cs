using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence.Repositories;

public sealed class RecurringExpenseRepository(ExpenseManagerDbContext db) : IRecurringExpenseRepository
{
    public async Task<RecurringExpense?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default)
        => await db.RecurringExpenses
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RecurringExpense>> ListAsync(string userId, CancellationToken cancellationToken = default)
        => await db.RecurringExpenses
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task AddAsync(RecurringExpense recurring, CancellationToken cancellationToken = default)
        => db.RecurringExpenses.AddAsync(recurring, cancellationToken).AsTask();

    public void Remove(RecurringExpense recurring) => db.RecurringExpenses.Remove(recurring);
}
