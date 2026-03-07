using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> ListAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Remove(Category category);
    Task<bool> ExistsWithNameAsync(string userId, string name, Guid? excludingId, CancellationToken cancellationToken = default);
}

