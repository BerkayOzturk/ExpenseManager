namespace ExpenseManager.Application.Common.Exceptions;

/// <summary>
/// Thrown when a domain rule is violated. Used so the API layer does not depend on Domain.
/// </summary>
public sealed class DomainRuleException : Exception
{
    public DomainRuleException(string message) : base(message) { }
}
