namespace ExpenseManager.Application.Features.Budgets;

public sealed record BudgetDto(
    Guid Id,
    Guid? CategoryId,
    string? CategoryName,
    decimal Amount,
    string Currency,
    int Year,
    int? Month);

public sealed record BudgetSummaryDto(
    Guid Id,
    Guid? CategoryId,
    string? CategoryName,
    decimal BudgetAmount,
    string Currency,
    int Year,
    int? Month,
    decimal Spent,
    bool IsOverBudget);
