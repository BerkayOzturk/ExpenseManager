using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRepository(ExpenseManagerDbContext db) : IExpenseRepository
{
    public async Task<Expense?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default)
        => await db.Expenses
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Expense>> ListAsync(
        string userId,
        DateOnly? from,
        DateOnly? to,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Expenses
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId);

        if (from is not null) query = query.Where(x => x.OccurredOn >= from.Value);
        if (to is not null) query = query.Where(x => x.OccurredOn <= to.Value);
        if (categoryId is not null) query = query.Where(x => x.CategoryId == categoryId);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalSpentAsync(string userId, string currency, int year, int? month, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var from = new DateOnly(year, month ?? 1, 1);
        var to = month.HasValue
            ? new DateOnly(year, month.Value, DateTime.DaysInMonth(year, month.Value))
            : new DateOnly(year, 12, 31);

        var query = db.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Currency == currency && x.OccurredOn >= from && x.OccurredOn <= to);

        if (categoryId is not null)
            query = query.Where(x => x.CategoryId == categoryId);

        var amounts = await query.Select(x => x.Amount).ToListAsync(cancellationToken);
        return amounts.Sum();
    }

    public Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
        => db.Expenses.AddAsync(expense, cancellationToken).AsTask();

    public void Remove(Expense expense) => db.Expenses.Remove(expense);
}

