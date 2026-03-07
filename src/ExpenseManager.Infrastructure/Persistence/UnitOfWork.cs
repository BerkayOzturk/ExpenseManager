using ExpenseManager.Application.Abstractions.Persistence;

namespace ExpenseManager.Infrastructure.Persistence;

public sealed class UnitOfWork(ExpenseManagerDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}

