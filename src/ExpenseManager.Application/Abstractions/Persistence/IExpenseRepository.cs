using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Abstractions.Persistence;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Expense> Items, int TotalCount)> ListWithCountAsync(string userId, DateOnly? from, DateOnly? to, Guid? categoryId, string? search, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalSpentAsync(string userId, string currency, int year, int? month, Guid? categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
    void Remove(Expense expense);
}

