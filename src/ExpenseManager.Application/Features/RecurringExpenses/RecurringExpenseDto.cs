namespace ExpenseManager.Application.Features.RecurringExpenses;

public sealed record RecurringExpenseDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly FirstOccurrenceOn,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    DateOnly? EndOn);

/// <summary>
/// A single generated occurrence from a recurring expense (for upcoming view).
/// </summary>
public sealed record UpcomingExpenseDto(
    DateOnly OccurredOn,
    decimal Amount,
    string Currency,
    string? Description,
    string? CategoryName,
    Guid RecurringExpenseId);
