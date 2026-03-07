using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        currency = currency.Trim().ToUpperInvariant();
        if (currency.Length is < 3 or > 5) throw new DomainException("Currency must be 3-5 chars (e.g. USD, EUR).");
        return new Money(amount, currency);
    }
}

