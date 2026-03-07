using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ExpenseManagerDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(string userId, Guid id, CancellationToken cancellationToken = default)
        => await db.Categories.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(string userId, CancellationToken cancellationToken = default)
        => await db.Categories.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => db.Categories.AddAsync(category, cancellationToken).AsTask();

    public void Remove(Category category) => db.Categories.Remove(category);

    public Task<bool> ExistsWithNameAsync(string userId, string name, Guid? excludingId, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        return db.Categories.AnyAsync(
            c => c.UserId == userId && c.Name.ToLower() == name.ToLower() && (excludingId == null || c.Id != excludingId),
            cancellationToken);
    }
}

