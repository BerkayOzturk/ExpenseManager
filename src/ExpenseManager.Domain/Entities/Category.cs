using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.Entities;

public sealed class Category : AuditableEntity
{
    private Category() { } // EF

    public Category(string name, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new DomainException("UserId is required.");
        UserId = userId;
        Rename(name);
    }

    public string UserId { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");
        Name = name.Trim();
        Touch();
    }
}

