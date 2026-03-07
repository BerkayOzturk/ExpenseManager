using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Abstractions.Persistence;

public interface IRecurringExpenseRepository
{
    Task<RecurringExpense?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringExpense>> ListAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(RecurringExpense recurring, CancellationToken cancellationToken = default);
    void Remove(RecurringExpense recurring);
}
