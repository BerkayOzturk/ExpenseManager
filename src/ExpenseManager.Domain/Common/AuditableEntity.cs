namespace ExpenseManager.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; protected set; }

    protected void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}

