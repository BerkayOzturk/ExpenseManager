using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExpenseManager.Infrastructure.Persistence;

/// <summary>
/// Ensures the database is created on startup. Keeps this concern inside Infrastructure (Onion).
/// Uses a scope to resolve scoped DbContext from singleton IHostedService.
/// If the DB already existed before Budgets was added, ensures the Budgets table exists.
/// </summary>
public sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExpenseManagerDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureCategoriesSortOrderColumnAsync(db, cancellationToken);
        await EnsureBudgetsTableExistsAsync(db, cancellationToken);
        await EnsureUserSettingsTableExistsAsync(db, cancellationToken);
        await EnsureRecurringExpensesTableExistsAsync(db, cancellationToken);
        await EnsurePasswordResetCodesTableExistsAsync(db, cancellationToken);
    }

    private static async Task EnsureCategoriesSortOrderColumnAsync(ExpenseManagerDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Categories ADD COLUMN SortOrder INTEGER NOT NULL DEFAULT 0",
                cancellationToken);
        }
        catch
        {
            // Column already exists (e.g. after schema update)
        }
    }

    private static async Task EnsureRecurringExpensesTableExistsAsync(ExpenseManagerDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS RecurringExpenses (
                Id TEXT NOT NULL PRIMARY KEY,
                UserId TEXT NOT NULL,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,
                FirstOccurrenceOn TEXT NOT NULL,
                Description TEXT NULL,
                CategoryId TEXT NULL,
                EndOn TEXT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
            )
            """,
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_RecurringExpenses_UserId ON RecurringExpenses(UserId)",
            cancellationToken);
    }

    private static async Task EnsurePasswordResetCodesTableExistsAsync(ExpenseManagerDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS PasswordResetCodes (
                Email TEXT NOT NULL PRIMARY KEY,
                Code TEXT NOT NULL,
                ResetToken TEXT NOT NULL,
                ExpiresAtUtc TEXT NOT NULL
            )
            """,
            cancellationToken);
    }

    private static async Task EnsureUserSettingsTableExistsAsync(ExpenseManagerDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS UserSettings (
                UserId TEXT NOT NULL PRIMARY KEY,
                DefaultCurrency TEXT NOT NULL,
                DateFormat TEXT NOT NULL,
                Theme TEXT NOT NULL,
                Language TEXT NULL
            )
            """,
            cancellationToken);
    }

    private static async Task EnsureBudgetsTableExistsAsync(ExpenseManagerDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Budgets (
                Id TEXT NOT NULL PRIMARY KEY,
                UserId TEXT NOT NULL,
                CategoryId TEXT NULL,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,
                Year INTEGER NOT NULL,
                Month INTEGER NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
            )
            """,
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_Budgets_UserId ON Budgets(UserId)",
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
