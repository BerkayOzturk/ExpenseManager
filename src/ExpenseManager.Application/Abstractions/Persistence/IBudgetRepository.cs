using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Abstractions.Persistence;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> ListAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    void Remove(Budget budget);
}
