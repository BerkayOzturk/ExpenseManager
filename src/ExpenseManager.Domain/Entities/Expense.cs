using ExpenseManager.Domain.Common;
using ExpenseManager.Domain.ValueObjects;

namespace ExpenseManager.Domain.Entities;

public sealed class Expense : AuditableEntity
{
    private Expense() { } // EF

    public Expense(
        string userId,
        Money money,
        DateOnly occurredOn,
        string? description,
        Guid? categoryId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new DomainException("UserId is required.");
        UserId = userId;
        SetMoney(money);
        SetOccurredOn(occurredOn);
        SetDescription(description);
        SetCategory(categoryId);
    }

    public string UserId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateOnly OccurredOn { get; private set; }
    public string? Description { get; private set; }

    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public void Update(Money money, DateOnly occurredOn, string? description, Guid? categoryId)
    {
        SetMoney(money);
        SetOccurredOn(occurredOn);
        SetDescription(description);
        SetCategory(categoryId);
        Touch();
    }

    private void SetMoney(Money money)
    {
        if (money.Amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(money.Currency)) throw new DomainException("Currency is required.");
        Amount = money.Amount;
        Currency = money.Currency.Trim().ToUpperInvariant();
    }

    private void SetOccurredOn(DateOnly occurredOn)
    {
        if (occurredOn > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw new DomainException("OccurredOn cannot be in the far future.");
        OccurredOn = occurredOn;
    }

    private void SetDescription(string? description)
    {
        if (description is null)
        {
            Description = null;
            return;
        }

        description = description.Trim();
        if (description.Length == 0)
        {
            Description = null;
            return;
        }

        if (description.Length > 500) throw new DomainException("Description cannot exceed 500 characters.");
        Description = description;
    }

    private void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }
}

