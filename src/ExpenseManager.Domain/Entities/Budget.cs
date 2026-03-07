using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.Entities;

public sealed class Budget : Entity
{
    private Budget() { } // EF

    public Budget(string userId, Guid? categoryId, decimal amount, string currency, int year, int? month)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new DomainException("UserId is required.");
        if (amount < 0) throw new DomainException("Budget amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (year < 2000 || year > 2100) throw new DomainException("Year must be between 2000 and 2100.");
        if (month is int m && (m < 1 || m > 12)) throw new DomainException("Month must be 1–12 or null for yearly.");

        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Year = year;
        Month = month;
    }

    public string UserId { get; private set; } = default!;
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public int Year { get; private set; }
    public int? Month { get; private set; }

    public void Update(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Budget amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }
}
