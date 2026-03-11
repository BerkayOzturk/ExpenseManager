using ExpenseManager.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Persistence;

public sealed class PasswordResetCodeStore(ExpenseManagerDbContext db) : IPasswordResetCodeStore
{
    public async Task StoreAsync(string email, string code, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        await db.PasswordResetCodes
            .Where(x => x.Email == email)
            .ExecuteDeleteAsync(cancellationToken);

        db.PasswordResetCodes.Add(new PasswordResetCode
        {
            Email = email,
            Code = code,
            ExpiresAtUtc = expiresAt,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryConsumeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var row = await db.PasswordResetCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email && x.Code == code, cancellationToken);

        if (row is null || row.ExpiresAtUtc < DateTimeOffset.UtcNow)
            return false;

        await db.PasswordResetCodes.Where(x => x.Email == email).ExecuteDeleteAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
