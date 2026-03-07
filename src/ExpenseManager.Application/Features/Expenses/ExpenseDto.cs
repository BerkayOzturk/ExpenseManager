namespace ExpenseManager.Application.Features.Expenses;

public sealed record ExpenseDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly OccurredOn,
    string? Description,
    Guid? CategoryId,
    string? CategoryName);

