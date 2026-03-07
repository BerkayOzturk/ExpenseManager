using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.Entities;

public sealed class RecurringExpense : Entity
{
    private RecurringExpense() { } // EF

    public RecurringExpense(string userId, decimal amount, string currency, DateOnly firstOccurrenceOn, string? description, Guid? categoryId, DateOnly? endOn = null)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new DomainException("UserId is required.");
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (firstOccurrenceOn == default) throw new DomainException("First occurrence date is required.");
        if (endOn.HasValue && endOn.Value < firstOccurrenceOn) throw new DomainException("End date cannot be before first occurrence.");

        UserId = userId;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        FirstOccurrenceOn = firstOccurrenceOn;
        Description = description?.Trim();
        if (Description?.Length == 0) Description = null;
        if (Description?.Length > 500) throw new DomainException("Description cannot exceed 500 characters.");
        CategoryId = categoryId;
        EndOn = endOn;
    }

    public string UserId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateOnly FirstOccurrenceOn { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public DateOnly? EndOn { get; private set; }

    public void Update(decimal amount, string currency, DateOnly firstOccurrenceOn, string? description, Guid? categoryId, DateOnly? endOn)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (endOn.HasValue && endOn.Value < firstOccurrenceOn) throw new DomainException("End date cannot be before first occurrence.");
        var desc = description?.Trim();
        if (desc?.Length > 500) throw new DomainException("Description cannot exceed 500 characters.");

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        FirstOccurrenceOn = firstOccurrenceOn;
        Description = desc?.Length == 0 ? null : desc;
        CategoryId = categoryId;
        EndOn = endOn;
    }
}
